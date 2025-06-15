using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllAsync();
    
    Task<IEnumerable<EventDTO>> GetAllWithDetailsAsync();

    Task<IEnumerable<EventDTO>> GetAllBySportAsync(string sportType);
    Task<EventDTO?> GetByIdAsync(int id);
    Task<EventDTO> CreateAsync(CreateEventDTO e);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Event>> GetByLocationIdAsync(int locationId);
    
    Task<bool> UpdateAsync(int id, CreateEventDTO dto);
    
    Task<FilterOptionsDTO> GetFilterOptionsAsync();


}