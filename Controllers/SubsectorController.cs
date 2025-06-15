using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Services;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubsectorController : ControllerBase
{
    private readonly ISubsectorService _service;

    public SubsectorController(ISubsectorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubsectorDTO>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubsectorDTO>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("by-sector/{sectorId}")]
    public async Task<ActionResult<IEnumerable<SubsectorDTO>>> GetBySectorId(int sectorId)
    {
        var result = await _service.GetBySectorIdAsync(sectorId);
        return Ok(result);
    }

    [HttpGet("by-location/{locationId}")]
    public async Task<ActionResult<IEnumerable<SubsectorDTO>>> GetByLocationId(int locationId)
    {
        var result = await _service.GetByLocationIdAsync(locationId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SubsectorDTO>> Create([FromBody] CreateSubsectorDTO dto)
    {
        try
        {
            var newDto = new SubsectorDTO
            {
                SubsectorName = dto.SubsectorName,
                SectorId = dto.SectorId,
                Rows = dto.Rows,
                SeatsPerRow = dto.SeatsPerRow
            };

            var result = await _service.CreateAsync(newDto);

            return CreatedAtAction(nameof(GetById), new { id = result.SubsectorId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Eroare: " + ex.Message);
        }
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSubsectorDTO dto)
    {
        try
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Eroare la actualizare subsector: " + ex.Message);
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
    
    [HttpGet("locked/{id}")]
    public async Task<IActionResult> IsLocked(int id)
    {
        var locked = await _service.IsLockedAsync(id);
        return Ok(new { locked });
    }

}