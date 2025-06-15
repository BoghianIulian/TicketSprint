using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ParticipantController : ControllerBase
{
    private readonly IParticipantService _service;
    private readonly IFavoriteService _favoriteService;

    public ParticipantController(IParticipantService service, IFavoriteService favoriteService)
    {
        _service = service;
        _favoriteService = favoriteService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Participant>>> GetAll()
    {
        var participants = await _service.GetAllAsync();
        var result = participants.Select(p => new Participant
        {
            ParticipantId = p.ParticipantId,
            Name = p.Name,
            SportType = p.SportType,
            ImageUrl = p.ImageUrl
        });

        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Participant>> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        if (p == null) return NotFound();

        return Ok(new Participant
        {
            ParticipantId = p.ParticipantId,
            Name = p.Name,
            SportType = p.SportType,
            ImageUrl = p.ImageUrl
        });
    }
    
    [HttpPost]
    public async Task<ActionResult<Participant>> Create([FromForm] CreateParticipantDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ParticipantId }, created);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "A apărut o eroare internă.");
        }
    }

    [HttpGet("{id}/events")]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents(int id)
    {
        var events = await _service.GetEventsByParticipantIdAsync(id);
        return Ok(events);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] CreateParticipantDTO dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    
    [HttpGet("suggestions/{userId}")]
    public async Task<ActionResult<IEnumerable<CreateParticipantDTO>>> GetSuggestions(int userId)
    {
        var favoriteIds = await _favoriteService.GetFavoriteParticipantIdsAsync(userId);
        var nonFavorites = await _service.GetNonFavoriteParticipantsAsync(favoriteIds);

        var result = nonFavorites.Select(p => new ParticipantDTO
        {
            ParticipantId = p.ParticipantId,
            Name = p.Name,
            SportType = p.SportType,
            ImageUrl = p.ImageUrl
        });

        return Ok(result);
    }
    
    [HttpGet("sports")]
    public async Task<ActionResult<IEnumerable<string>>> GetDistinctSports()
    {
        var sports = await _service.GetDistinctSportsAsync();
        return Ok(sports);
    }
    
    [HttpGet("by-sport/{sport}")]
    public async Task<ActionResult<IEnumerable<ParticipantDTO>>> GetBySport(string sport)
    {
        var result = await _service.GetBySportAsync(sport);
        return Ok(result.Select(p => new ParticipantDTO
        {
            ParticipantId = p.ParticipantId,
            Name = p.Name,
            SportType = p.SportType,
            ImageUrl = p.ImageUrl
        }));
    }
    
    [HttpGet("by-participant/{id}")]
    public async Task<IActionResult> GetEventsByParticipantId(int id)
    {
        var events = await _service.GetEventsByParticipantIdAsync(id);
        return Ok(events);
    }




    
}