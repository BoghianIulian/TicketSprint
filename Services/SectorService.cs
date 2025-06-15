using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class SectorService : ISectorService
{
    private readonly ISectorRepository _repository;
    private readonly IEventRepository _eventRepository;

    public SectorService(ISectorRepository repository, IEventRepository eventRepository)
    {
        _repository = repository;
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Sector>> GetAllAsync() => await _repository.GetAllAsync();

    public async Task<Sector?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<Sector>> GetByLocationIdAsync(int locationId)
        => await _repository.GetByLocationIdAsync(locationId);

    public async Task<Sector> CreateAsync(Sector sector)
    {
        var exists = await _repository.ExistsWithNameInLocationAsync(sector.SectorName, sector.LocationId);
        if (exists)
            throw new InvalidOperationException("Există deja un sector cu acest nume în locația selectată.");

        await _repository.AddAsync(sector);
        return sector;
    }

    public async Task<bool> UpdateAsync(int id, UpdateSectorDTO updated)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        // verificăm dacă alt sector are deja același nume în acea locație
        var all = await _repository.GetAllAsync();
        bool duplicateExists = all.Any(s =>
            s.SectorName == updated.SectorName &&
            s.LocationId == updated.LocationId &&
            s.SectorId != id);

        if (duplicateExists)
            throw new InvalidOperationException("Există deja un alt sector cu acest nume în aceeași locație.");

        // dacă e ok, facem update-ul
        existing.SectorName = updated.SectorName;
        existing.LocationId = updated.LocationId;

        await _repository.UpdateAsync(existing);
        return true;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var sector = await _repository.GetByIdAsync(id);
        if (sector == null) return false;

        var subsectors = sector.Subsectors; 

        foreach (var ss in subsectors)
        {
            if (await _eventRepository.HasActiveFutureEventsBySubsectorIdAsync(ss.SubsectorId))
                throw new InvalidOperationException("Sectorul nu poate fi șters. Cel puțin un subsector este asociat cu un eveniment viitor activ.");
        }

        await _repository.DeleteAsync(id);
        return true;
    }
    public async Task<bool> IsLockedAsync(int sectorId)
    {
        return await _repository.IsLockedAsync(sectorId);
    }

}