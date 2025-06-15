using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _service;
    private readonly AppDbContext _context;
    private readonly ILocationService _locationService;

    public EventController(IEventService service, AppDbContext context, ILocationService locationService)
    {
        _service = service;
        _context = context;
        _locationService = locationService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDTO>>> GetAll()
    {
        var result = await _service.GetAllWithDetailsAsync();
        return Ok(result);
    }
    
    


    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDTO>> GetById(int id)
    {
        var eventDto = await _service.GetByIdAsync(id);
        if (eventDto == null) return NotFound();

        return Ok(eventDto);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateEventDTO dto)
    {
        try
        {
            var createdEvent = await _service.CreateAsync(dto);
            return Ok(createdEvent); // trimite obiectul complet înapoi la frontend
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    
    [HttpGet("bysport/{sport}")]
    public async Task<ActionResult<IEnumerable<EventDTO>>> GetBySportType(string sport)
    {
        var events = await _service.GetAllBySportAsync(sport);
        return Ok(events);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] CreateEventDTO dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated != null ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent(); 
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); 
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, "A apărut o eroare internă.");
        }
    }
    
    [HttpGet("filters")]
    public async Task<ActionResult<FilterOptionsDTO>> GetFilterOptions()
    {
        var filters = await _service.GetFilterOptionsAsync();
        return Ok(filters);
    }
    
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<EventDTO>>> FilterEvents(
        [FromQuery] string? sportsJson,
        [FromQuery] string? city,
        [FromQuery] string? location,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var events = (await _service.GetAllWithDetailsAsync()).ToList();

        Console.WriteLine($"StartDate: {startDate?.ToString("yyyy-MM-dd")}, EndDate: {endDate?.ToString("yyyy-MM-dd")}");

        //  Parsează lista de sporturi
        List<string> selectedSports = new();
        if (!string.IsNullOrEmpty(sportsJson))
        {
            try
            {
                selectedSports = JsonSerializer.Deserialize<List<string>>(sportsJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la parsarea sportsJson: " + ex.Message);
            }
        }

        //  Filtrare după city
        if (!string.IsNullOrEmpty(city))
        {
            var filteredByCity = new List<EventDTO>();

            foreach (var ev in events)
            {
                var loc = await _locationService.GetByNameAsync(ev.LocationName);
                if (loc != null && loc.City == city)
                {
                    filteredByCity.Add(ev);
                }
            }

            events = filteredByCity;
        }

        //  Filtrare după sport + locație + dată
        var filtered = events.Where(e =>
            (selectedSports.Count == 0 || selectedSports.Contains(e.SportType)) &&
            (string.IsNullOrEmpty(location) || e.LocationName == location) &&
            (!startDate.HasValue || e.EventDate >= startDate.Value) &&
            (!endDate.HasValue || e.EventDate <= endDate.Value)
        ).ToList();

        return Ok(filtered);
    }

    
    

}