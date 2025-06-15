using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface IParticipantRepository : IGenericRepository<Participant>
{
    Task<IEnumerable<Event>> GetEventsByParticipantIdAsync(int participantId);
    
    Task<List<string>> GetDistinctSportsAsync();
    
    Task<List<Participant>> GetBySportAsync(string sport);
    Task<bool> DeleteAsync(int id);


}