using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface IFavoriteRepository : IGenericRepository<Favorite>
{
    Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
    
    Task<bool> ExistsAsync(int userId, int participantId);
    
    Task<Favorite?> GetByUserAndParticipantAsync(int userId, int participantId);
    Task<Dictionary<int, int>> GetFrequentParticipantStatsAsync(string email);


}