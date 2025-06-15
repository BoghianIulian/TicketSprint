using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface IEventSectorRepository : IGenericRepository<EventSector>
{
    Task<IEnumerable<EventSector>> GetByEventIdAsync(int eventId);
    Task<EventSector?> GetByIdWithSubsectorAsync(int id);
    Task UpdateAsync(EventSector sector);
    Task<EventSector?> GetByEventIdAndSubsectorIdAsync(int eventId, int subsectorId);

    

}