using TicketSprint.Model;

namespace TicketSprint.Services;

public interface IEventSectorService
{
    Task<IEnumerable<EventSector>> GetAllAsync();
    Task<EventSector?> GetByIdAsync(int id);
    Task<IEnumerable<EventSector>> GetByEventIdAsync(int eventId);
    Task<EventSector> CreateAsync(EventSector es);
    Task<bool> DeleteAsync(int id);

    Task<EventSector?> GetByIdWithSubsectorAsync(int id);
    Task UpdateAsync(EventSector sector);

}