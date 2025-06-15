using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;
using TicketSprint.Utils;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IEventSectorService _eventSectorService;
    private readonly EmailService _emailService;

    public TicketController(ITicketService ticketService, IEventSectorService eventSectorService, EmailService emailService)
    {
        _ticketService = ticketService;
        _eventSectorService = eventSectorService;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDTO>>> GetAll()
    {
        return Ok(await _ticketService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDTO>> GetById(int id)
    {
        var result = await _ticketService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-eventsector/{eventSectorId}")]
    public async Task<ActionResult<IEnumerable<TicketDTO>>> GetByEventSector(int eventSectorId)
    {
        return Ok(await _ticketService.GetByEventSectorIdAsync(eventSectorId));
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByEmail(string email)
    {
        var ticketDtos = await _ticketService.GetByEmailAsync(email);

        var result = new List<object>();

        foreach (var t in ticketDtos)
        {
            // Luăm EventSector complet cu Event, Subsector, Sector
            var eventSector = await _eventSectorService.GetByIdWithSubsectorAsync(t.EventSectorId);
            if (eventSector == null || eventSector.Event == null || eventSector.Subsector?.Sector == null)
                continue;

            result.Add(new
            {
                TicketId = t.TicketId,
                EventName = eventSector.Event.EventName,
                EventDate = eventSector.Event.EventDate,
                LocationName = eventSector.Event.Location?.LocationName,
                ImageUrl = eventSector.Event.ImageUrl,

                SectorName = eventSector.Subsector.Sector.SectorName,
                SubsectorName = eventSector.Subsector.SubsectorName,

                Row = t.Row,
                Seat = t.Seat,
                Price = eventSector.Price,
                EventId = eventSector.EventId
            });
        }

        return Ok(result);
    }


    [HttpPost]
    public async Task<ActionResult<TicketDTO>> Create(CreateTicketDTO dto)
    {
        var created = await _ticketService.CreateAsync(dto);
        
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartId = Request.Headers["X-CartId"].ToString(); // îl trimiți din frontend în header

        TemporaryReservationStore.RemoveReservation(userId, cartId, dto.EventSectorId, dto.Row, dto.Seat);
        
        var sector = await _eventSectorService.GetByIdAsync(dto.EventSectorId);
        if (sector != null)
        {
            sector.AvailableSeats = Math.Max(0, sector.AvailableSeats - 1); 
            await _eventSectorService.UpdateAsync(sector);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = created.TicketId }, created);
    }
    
    [HttpPost("multiple")]
    public async Task<ActionResult<List<TicketDTO>>> CreateMultiple([FromBody] List<CreateTicketDTO> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return BadRequest("Lista de bilete este goală.");

        var createdTickets = new List<TicketDTO>();
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartId = Request.Headers["X-CartId"].ToString();

        foreach (var dto in dtos)
        {
            var created = await _ticketService.CreateAsync(dto);
            createdTickets.Add(created);

            TemporaryReservationStore.RemoveReservation(userId, cartId, dto.EventSectorId, dto.Row, dto.Seat);

            var sector = await _eventSectorService.GetByIdAsync(dto.EventSectorId);
            if (sector != null)
            {
                sector.AvailableSeats = Math.Max(0, sector.AvailableSeats - 1);
                await _eventSectorService.UpdateAsync(sector);
            }
        }
        
        var fullTickets = new List<(TicketDTO Simple, Ticket FullModel)>();

        foreach (var ticket in createdTickets)
        {
            var full = await _ticketService.GetFullTicketModelAsync(ticket.TicketId);
            if (full != null)
                fullTickets.Add((ticket, full));
        }


        // ===  Trimitere pe email în fundal ===
        _ = Task.Run(async () =>
        {
            try
            {
                var attachments = new List<(string, byte[])>();

                foreach (var (simple, model) in fullTickets)
                {
                    var pdf = PdfGenerator.GenerateTicketPdf(model);
                    attachments.Add(($"ticket_{simple.TicketId}.pdf", pdf));
                }

                var first = fullTickets.First().Simple;
                await _emailService.SendTicketsEmail(first.Email, attachments, first.FirstName, first.LastName);
                Console.WriteLine("[✓] Email trimis.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Eroare la trimiterea emailului: " + ex.Message);
            }
        });


        return Ok(createdTickets);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticketDto = await _ticketService.GetByIdAsync(id);
        if (ticketDto == null) return NotFound();

        var success = await _ticketService.DeleteAsync(id);
        if (!success) return NotFound();

        var sector = await _eventSectorService.GetByIdAsync(ticketDto.EventSectorId);
        if (sector != null)
        {
            sector.AvailableSeats += 1;
            await _eventSectorService.UpdateAsync(sector);
        }

        return NoContent();
    }
    [HttpGet("occupied-seats/{eventSectorId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetOccupiedSeats(
        int eventSectorId,
        [FromQuery] string? userId,
        [FromQuery] string? cartId)
    {
        var result = new List<object>();

        
        var tickets = await _ticketService.GetByEventSectorIdAsync(eventSectorId);
        result.AddRange(tickets.Select(t => new { t.Row, t.Seat, Type = "occupied" }));

        
        var reservations = TemporaryReservationStore.GetValidForSector(eventSectorId);

        foreach (var r in reservations)
        {
            var type = (userId != null && r.UserId == userId) || (cartId != null && r.CartId == cartId)
                ? "in-cart"
                : "occupied";

            result.Add(new { r.Row, r.Seat, Type = type });
        }

        return Ok(result);
    }
    
    [HttpGet("admin/occupied-seats/{eventSectorId}")]
    public async Task<ActionResult<IEnumerable<TicketWithRoleDTO>>> GetOccupiedSeatsForAdmin(int eventSectorId)
    {
        var result = await _ticketService.GetOccupiedSeatsWithRoleForAdmin(eventSectorId);
        return Ok(result);
    }

    
    [HttpPost("finalize")]
    public async Task<IActionResult> FinalizePurchase([FromBody] FinalizePurchaseDTO dto)
    {
        try
        {
            MailboxAddress.Parse(dto.Email);
        }
        catch
        {
            return BadRequest("Emailul introdus nu este valid.");
        }

        var tickets = new List<TicketDTO>();

        foreach (var id in dto.TicketIds.Distinct())
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket != null && ticket.Email == dto.Email)
            {
                tickets.Add(ticket);
            }
        }

        if (!tickets.Any())
            return BadRequest("Nu s-au găsit bilete pentru această comandă.");
        Console.WriteLine($"Trimit email la: {dto.Email} pentru {tickets.Count} bilete");


        // Generăm PDF-uri
        var attachments = new List<(string, byte[])>();
        foreach (var ticket in tickets)
        {
            var model = await _ticketService.GetFullTicketModelAsync(ticket.TicketId);
            if (model == null) continue;

            var pdf = PdfGenerator.GenerateTicketPdf(model);
            attachments.Add(($"ticket_{ticket.TicketId}.pdf", pdf));
        }

        _ = Task.Run(async () =>
        {
            var emailService = new EmailService();
            try
            {
                await emailService.SendTicketsEmail(
                    dto.Email,
                    attachments,
                    tickets.First().FirstName,
                    tickets.First().LastName
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la trimiterea emailului: " + ex.Message);
            }
        });

// ✅ Trimitem răspuns imediat către frontend
        return Ok("Comanda a fost înregistrată! Vei primi biletele pe email în câteva secunde.");
    }
    
    [HttpGet("by-qrcode")]
    public async Task<IActionResult> GetByQRCode([FromQuery] string code)
    {
        var ticket = await _ticketService.GetByQRCodeAsync(code);
        if (ticket == null)
            return NotFound("Nu s-a găsit niciun bilet pentru acest cod.");

        return Ok(new
        {
            ticket.TicketId,
            ticket.EventSectorId,
            ticket.Row,
            ticket.Seat,
            ticket.Email,
            ticket.FirstName,
            ticket.LastName,
            ticket.EventSector?.Event?.EventName
        });
    }
    
    




}