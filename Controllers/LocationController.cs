using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationController(ILocationService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetAll()
    {
        var locations = await _service.GetAllAsync();
        return Ok(locations.Select(l => new Location
        {
            LocationId = l.LocationId,
            LocationName = l.LocationName,
            LocationType = l.LocationType,
            City = l.City,
            Capacity = l.Capacity
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetById(int id)
    {
        var loc = await _service.GetByIdAsync(id);
        if (loc == null) return NotFound();

        return Ok(new Location
        {
            LocationId = loc.LocationId,
            LocationName = loc.LocationName,
            LocationType = loc.LocationType,
            City = loc.City,
            Capacity = loc.Capacity
        });
    }
    
    [HttpPost]
    public async Task<ActionResult<Location>> Create(LocationDTO dto)
    {
        if (dto == null)
            return BadRequest("Datele locației sunt invalide.");

        try
        {
            var location = new Location
            {
                LocationName = dto.LocationName,
                LocationType = dto.LocationType,
                City = dto.City,
                Capacity = dto.Capacity
            };

            var created = await _service.CreateAsync(location);
            return CreatedAtAction(nameof(GetById), new { id = created.LocationId }, created);
        }
        catch(InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch  (Exception ex)
        {
            return StatusCode(500, "Eroare neașteptată: " + ex.Message);
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LocationDTO dto)
    {
        if (dto == null)
            return BadRequest("Datele locației sunt invalide.");

        try
        {
            var location = new Location
            {
                LocationName = dto.LocationName,
                LocationType = dto.LocationType,
                City = dto.City,
                Capacity = dto.Capacity
            };

            var result = await _service.UpdateAsync(id, location);
            return result ? NoContent() : NotFound();
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, "Eroare la actualizarea locației: " + ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message); // 409 Conflict: nume duplicat
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Eroare neașteptată: " + ex.Message);
        }
        
        
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { message = ex.Message }) { StatusCode = 400 };
        }
        catch
        {
            return StatusCode(500, "A apărut o eroare internă.");
        }
    }

}