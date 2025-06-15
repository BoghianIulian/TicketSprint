using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface IEventRepository : IGenericRepository<Event>
{
    Task<IEnumerable<Event>> GetByLocationIdAsync(int locationId);
    
    Task<bool> UpdateAsync(Event updatedEvent);
    
    Task<Event> CreateAsync(Event e);
    Task<Participant?> GetParticipantByNameAndSport(string name, string sport);
    Task<Location?> GetLocationByName(string name);
    Task<bool> CheckConflict(DateTime date, int locationId);
    
    Task<FilterOptionsDTO> GetFilterOptionsAsync();
    
    Task<List<SuggestedEventDTO>> GetEventsByParticipantsAsync(List<int> participantIds);

    Task<bool> HasActiveFutureEventsBySubsectorIdAsync(int subsectorId);

    Task<bool> HasFutureEventsByLocationIdAsync(int locationId);
    
    


}