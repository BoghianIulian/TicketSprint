using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;

    public LocationService(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Location>> GetAllAsync() => await _repository.GetAllAsync();

    public async Task<Location?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task<Location> CreateAsync(Location location)
    {
        var existing = await _repository.GetByNameAsync(location.LocationName);
        if (existing != null)
            throw new InvalidOperationException("Există deja o locație cu acest nume.");

        await _repository.AddAsync(location);
        return location;
    }

    public async Task<bool> UpdateAsync(int id, Location updatedLocation)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;
        var duplicate = await _repository.GetByNameAsync(updatedLocation.LocationName);
        if (duplicate != null && duplicate.LocationId != id)
            throw new InvalidOperationException("Există deja o altă locație cu acest nume.");

        existing.LocationName = updatedLocation.LocationName;
        existing.LocationType = updatedLocation.LocationType;
        existing.City = updatedLocation.City;
        existing.Capacity = updatedLocation.Capacity;

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteIfNoFutureEventsAsync(id);
    }   
    
    public async Task<Location?> GetByNameAsync(string name)
    {
        return await _repository.GetByNameAsync(name);
    }

}