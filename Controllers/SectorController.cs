using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SectorController : ControllerBase
{
    private readonly ISectorService _service;
    private readonly AppDbContext _context;

    public SectorController(ISectorService service , AppDbContext context)
    {
        _service = service;
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sector>>> GetAll()
    {
        var sectors = await _service.GetAllAsync();
        return Ok(sectors);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Sector>> GetById(int id)
    {
        var sector = await _service.GetByIdAsync(id);
        if (sector == null) return NotFound();
        return Ok(sector);
    }
    
    [HttpGet("by-location/{locationId}")]
    public async Task<ActionResult<IEnumerable<Sector>>> GetByLocationId(int locationId)
    {
        var sectors = await _service.GetByLocationIdAsync(locationId);
        return Ok(sectors);
    }
    
    [HttpPost]
    public async Task<ActionResult<Sector>> Create([FromBody] CreateSectorDTO dto)
    {
        try
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationName == dto.LocationName);

            if (location == null)
                return NotFound($"Locația '{dto.LocationName}' nu a fost găsită.");

            var sector = new Sector
            {
                SectorName = dto.SectorName,
                LocationId = location.LocationId
            };

            var created = await _service.CreateAsync(sector);

            return CreatedAtAction(nameof(GetById), new { id = created.SectorId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Eroare la creare sector: " + ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSectorDTO sector)
    {
        try
        {
            
            
            var result = await _service.UpdateAsync(id, sector);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Eroare la actualizare sector: " + ex.Message);
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
    
    [HttpGet("locked/{id}")]
    public async Task<IActionResult> IsLocked(int id)
    {
        var locked = await _service.IsLockedAsync(id);
        return Ok(new { locked });
    }


    
}