using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventSectorController : ControllerBase
{
    private readonly IEventSectorService _service;
    private readonly ISubsectorService _subsectorService; 

    public EventSectorController(IEventSectorService service, ISubsectorService subsectorService)
    {
        _service = service;
        _subsectorService = subsectorService;
        
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventSector>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventSector>> GetById(int id)
    {
        var es = await _service.GetByIdAsync(id);
        return es == null ? NotFound() : Ok(es);
    }

    [HttpGet("event/display/{eventId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByEventId(int eventId)
    {
        var list = await _service.GetByEventIdAsync(eventId);

        // direct aici proiectezi către un obiect anonim
        var result = list.Select(es => new
        {
            es.EventSectorId,
            es.SubsectorId,
            SubsectorName = es.Subsector?.SubsectorName,
            SectorName = es.Subsector?.Sector?.SectorName,
            es.Price,
            es.IsActive,
            es.AvailableSeats
        });

        return Ok(result);
    }


    [HttpPost]
    public async Task<ActionResult<EventSector>> Create(EventSectorDTO dto)
    {
        var subsector = await _subsectorService.GetByIdAsync(dto.SubsectorId);
        if (subsector == null)
            return BadRequest("Subsectorul nu există.");

        // 2. Calculează locurile
        var availableSeats = subsector.Rows * subsector.SeatsPerRow;
        var entity = new EventSector
        {
            EventId = dto.EventId,
            SubsectorId  = dto.SubsectorId,
            AvailableSeats = availableSeats,
            Price = dto.Price,
            IsActive = dto.IsActive
        };

        var created = await _service.CreateAsync(entity);

        var response = new EventSectorDTO
        {
            EventId = created.EventId,
            SubsectorId = created.SubsectorId,
            Price = created.Price,
            IsActive = created.IsActive
        };

        return CreatedAtAction(nameof(GetById), new { id = created.EventSectorId }, response);    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
    
    [HttpGet("config/{eventSectorId}")]
    public async Task<IActionResult> GetSeatConfig(int eventSectorId)
    {
        var es = await _service.GetByIdAsync(eventSectorId);
        
        var sector = await _service.GetByIdAsync(eventSectorId);
        if (sector == null || sector.Subsector == null)
            return NotFound("Sectorul nu a fost găsit.");

        if (es == null)
            return NotFound("EventSector inexistent.");

        if (es.Subsector == null)
            return NotFound("EventSector nu are Subsector asociat.");

        return Ok(new
        {
            rows = es.Subsector.Rows,
            seatsPerRow = es.Subsector.SeatsPerRow,
            subsectorName =  sector.Subsector.SubsectorName ,
            price = sector.Price
        });
    }
    
    [HttpPut]
    [HttpPut]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] EventSectorDTO dto)
    {
        var sector = new EventSector
        {
            EventId = dto.EventId,
            SubsectorId = dto.SubsectorId,
            Price = dto.Price,
            IsActive = dto.IsActive
        };

        await _service.UpdateAsync(sector);
        return Ok();
    }



}