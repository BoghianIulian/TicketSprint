using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoriteController:ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    // GET: api/Favorite/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<FavoriteDTO>>> GetByUserId(int userId)
    {
        var favorites = await _favoriteService.GetFavoriteDTOsByUserAsync(userId);
        return Ok(favorites);
    }
    
    [HttpPost]
    public async Task<ActionResult<FavoriteDTO>> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        int participantId = request.ParticipantId;

        Console.WriteLine($"[AddFavorite] UserId: {userId}, ParticipantId: {participantId}");

        var dto = new FavoriteDTO
        {
            UserId = userId,
            ParticipantId = participantId
        };

        var result = await _favoriteService.AddFavoriteAsync(dto);

        if (result == null)
            return BadRequest("Participantul nu există sau este deja favorit pentru acest utilizator.");

        return CreatedAtAction(nameof(GetByUserId), new { userId = userId }, result);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var success = await _favoriteService.DeleteAsync(id);
        if (!success)
            return NotFound("Favorite-ul nu a fost găsit.");

        return NoContent();
    }
    
    [HttpGet("suggestions")]
    public async Task<ActionResult<IEnumerable<EventDTO>>> GetSuggestions()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
        Console.WriteLine("UserId din claims: " + userIdClaim); // ADĂUGAT

        if (!int.TryParse(userIdClaim, out var userId))
            return Ok(new List<EventDTO>());

        var suggestions = await _favoriteService.GetSuggestedEventsForUserAsync(userId);
        return Ok(suggestions);
    }
    
    [HttpGet("exists")]
    public async Task<IActionResult> IsFavorite([FromQuery] int participantId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        bool isFavorite = await _favoriteService.ExistsAsync(userId, participantId);

        return Ok(isFavorite);
    }
    
    [HttpGet("by-user-participant")]
    public async Task<ActionResult<Favorite>> GetByUserAndParticipant([FromQuery] int participantId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        var favorite = await _favoriteService.GetByUserAndParticipantAsync(userId, participantId);

        if (favorite == null)
            return NotFound();

        return Ok(favorite);
    }
    
    [HttpGet("frequent-participants/{userId}")]
    public async Task<ActionResult<Dictionary<int, int>>> GetFrequentParticipants(int userId)
    {
        var result = await _favoriteService.GetFrequentParticipantsAsync(userId);
        return Ok(result);
    }





}