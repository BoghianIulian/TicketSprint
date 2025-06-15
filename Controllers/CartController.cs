using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Services;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly IEventSectorService _eventSectorService;
    private readonly ITicketService _ticketService;

    public CartController(IEventSectorService eventSectorService, ITicketService ticketService)
    {
        _eventSectorService = eventSectorService;
        _ticketService = ticketService;
    }

    [HttpPost("temp-block")]
    public async Task<IActionResult> TempBlock([FromBody] TempCartRequestDTO dto)
    {
        var eventSector = await _eventSectorService.GetByIdWithSubsectorAsync(dto.EventSectorId);

        if (eventSector == null)
            return NotFound();

        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var generatedTickets = new List<CartTicketDTO>();

        foreach (var seat in dto.Seats)
        {
            //  Salvăm rezervarea temporară în memorie
            TemporaryReservationStore.Add(new TemporaryReservation
            {
                UserId = dto.UserId,
                CartId = dto.CartId,
                EventSectorId = dto.EventSectorId,
                Row = seat.Row,
                Seat = seat.Seat,
                ExpiresAt = expiresAt
            });

            //  Pregătim biletul pentru returnat în frontend
            generatedTickets.Add(new CartTicketDTO
            {
                EventName = eventSector.Event.EventName,
                EventDate = eventSector.Event.EventDate,
                LocationName = eventSector.Event.Location.LocationName,
                SubsectorName = eventSector.Subsector.SubsectorName,
                SectorName = eventSector.Subsector.Sector.SectorName,
                Row = seat.Row,
                Seat = seat.Seat,
                Price = eventSector.Price,
                ImageUrl = eventSector.Event.ImageUrl,
                ExpiresAt = expiresAt,
                UserId = dto.UserId,
                CartId = dto.CartId
            });
        }

        return Ok(generatedTickets);
    }
    
    [HttpGet]
    public IActionResult GetUserCart([FromQuery] string? userId, [FromQuery] string? cartId)
    {
        var userTickets = new List<TemporaryReservation>();

        if (!string.IsNullOrEmpty(userId))
        {
            userTickets = TemporaryReservationStore.GetUserReservations(userId);
        }
        else if (!string.IsNullOrEmpty(cartId))
        {
            userTickets = TemporaryReservationStore.GetCartReservations(cartId);
        }

        
        var now = DateTime.UtcNow;
        var validTickets = userTickets
            .Where(r => r.ExpiresAt > now)
            .ToList();

        return Ok(validTickets);
    }
    
    [HttpPost("remove-temp-reservation")]
    public IActionResult RemoveTempReservation([FromBody] TempCartRequestDTO dto)
    {
        foreach (var seat in dto.Seats)
        {
            TemporaryReservationStore.RemoveReservation(dto.UserId, dto.CartId, dto.EventSectorId, seat.Row, seat.Seat);
        }

        return Ok(new { message = "Rezervarea a fost eliminată." });
    }

}