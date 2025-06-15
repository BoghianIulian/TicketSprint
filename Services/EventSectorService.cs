using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class EventSectorService : IEventSectorService
{
    private readonly IEventSectorRepository _repository;

    public EventSectorService(IEventSectorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EventSector>> GetAllAsync() => await _repository.GetAllAsync();

    public async Task<EventSector?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<EventSector>> GetByEventIdAsync(int eventId) =>
        await _repository.GetByEventIdAsync(eventId);

    public async Task<EventSector> CreateAsync(EventSector es)
    {
        await _repository.AddAsync(es);
        return es;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }
    
    public async Task<EventSector?> GetByIdWithSubsectorAsync(int id)
    {
        return await _repository.GetByIdWithSubsectorAsync(id);
    }
    
    public async Task UpdateAsync(EventSector sector)
    {
        var existing = await _repository.GetByEventIdAndSubsectorIdAsync(sector.EventId, sector.SubsectorId);

        if (existing == null)
            throw new Exception("EventSector not found.");

        existing.Price = sector.Price;
        existing.IsActive = sector.IsActive;

        await _repository.UpdateAsync(existing);
    }



}