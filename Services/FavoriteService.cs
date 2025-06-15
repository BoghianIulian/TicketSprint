using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _repository;
    private readonly AppDbContext _context;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public FavoriteService(IFavoriteRepository repository, AppDbContext context, IEventRepository eventRepository, IUserRepository userRepository)
    {
        _repository = repository;
        _context = context;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<Favorite>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Favorite> CreateAsync(Favorite f)
    {
        await _repository.AddAsync(f);
        return f;
    }

    public async Task<bool> UpdateAsync(int id, Favorite updated)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.UserId = updated.UserId;
        existing.ParticipantId = updated.ParticipantId;

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }
    
    public async Task<IEnumerable<FavoriteDTO>> GetFavoriteDTOsByUserAsync(int userId)
    {
        var favorites = await _repository.GetByUserIdAsync(userId);

        return favorites.Select(f => new FavoriteDTO
        {
            FavoriteId = f.FavoriteId,
            UserId = f.UserId,
            ParticipantId = f.ParticipantId,
            ParticipantName = f.Participant?.Name ?? "",
            SportType = f.Participant?.SportType ?? "",
            ImageUrl = f.Participant?.ImageUrl
        });
    }

    public async Task<FavoriteDTO?> AddFavoriteAsync(FavoriteDTO dto)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.ParticipantId == dto.ParticipantId);

        if (participant == null)
            return null;

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == dto.UserId && f.ParticipantId == participant.ParticipantId);

        if (exists)
            return null;

        var favorite = new Favorite
        {
            UserId = dto.UserId,
            ParticipantId = participant.ParticipantId
        };

        await _repository.AddAsync(favorite);

        return new FavoriteDTO
        {
            FavoriteId = favorite.FavoriteId,
            UserId = favorite.UserId,
            ParticipantId = participant.ParticipantId,
            ParticipantName = participant.Name,
            SportType = participant.SportType,
            ImageUrl = participant.ImageUrl
        };
    }
    
    public async Task<HashSet<int>> GetFavoriteParticipantIdsAsync(int userId)
    {
        var favorites = await _repository.GetByUserIdAsync(userId);
        return favorites.Select(f => f.ParticipantId).ToHashSet();
    }
    public async Task<List<SuggestedEventDTO>> GetSuggestedEventsForUserAsync(int userId)
    {
        var favorites = await _repository.GetByUserIdAsync(userId);
        var participantIds = favorites.Select(f => f.ParticipantId).Distinct().ToList();

        if (!participantIds.Any())
            return new List<SuggestedEventDTO>();

        var events = await _eventRepository.GetEventsByParticipantsAsync(participantIds);

        
        
        var upcoming = events
            .Where(e => e.EventDate > DateTime.Now)
            .DistinctBy(e => e.EventId)
            .ToList();

        return upcoming;
    }
    
    public async Task<bool> ExistsAsync(int userId, int participantId)
    {
        return await _repository.ExistsAsync(userId, participantId);
    }
    
    public async Task<Favorite?> GetByUserAndParticipantAsync(int userId, int participantId)
    {
        return await _repository.GetByUserAndParticipantAsync(userId, participantId);
    }
    public async Task<Dictionary<int, int>> GetFrequentParticipantsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return new();

        return await _repository.GetFrequentParticipantStatsAsync(user.Email);
    }



    
    

    
   


}