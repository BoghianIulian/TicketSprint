using TicketSprint.Model;

namespace TicketSprint.Services;

public interface ILocationService
{
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task<Location> CreateAsync(Location location);
    Task<bool> UpdateAsync(int id, Location location);
    Task<bool> DeleteAsync(int id);
    Task<Location?> GetByNameAsync(string name);

}