using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Services;

public interface IFavoriteService
{
    Task<IEnumerable<Favorite>> GetAllAsync();
    Task<Favorite?> GetByIdAsync(int id);
    Task<Favorite> CreateAsync(Favorite f);
    Task<bool> UpdateAsync(int id, Favorite updated);
    Task<bool> DeleteAsync(int id);
        
    
    Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
    Task<IEnumerable<FavoriteDTO>> GetFavoriteDTOsByUserAsync(int userId);
    Task<FavoriteDTO?> AddFavoriteAsync(FavoriteDTO dto);

    Task<HashSet<int>> GetFavoriteParticipantIdsAsync(int userId);
    
    Task<List<SuggestedEventDTO>> GetSuggestedEventsForUserAsync(int userId);
    
    Task<bool> ExistsAsync(int userId, int participantId);
    Task<Favorite?> GetByUserAndParticipantAsync(int userId, int participantId);
    Task<Dictionary<int, int>> GetFrequentParticipantsAsync(int userId);





}