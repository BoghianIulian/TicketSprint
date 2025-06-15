using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Services;

public interface IParticipantService
{
    Task<IEnumerable<Participant>> GetAllAsync();
    Task<Participant?> GetByIdAsync(int id);
    Task<Participant> CreateAsync(CreateParticipantDTO dto);
    Task<bool> DeleteAsync(int id);
    
    Task<bool> UpdateAsync(int id , CreateParticipantDTO dto);

    Task<IEnumerable<EventDTO>> GetEventsByParticipantIdAsync(int participantId);

    Task<IEnumerable<Participant>> GetNonFavoriteParticipantsAsync(HashSet<int> favoriteIds);
    
    Task<List<string>> GetDistinctSportsAsync();
    
    Task<List<Participant>> GetBySportAsync(string sport);
    
    


}