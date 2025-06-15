using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface ISectorRepository : IGenericRepository<Sector>
{
    Task<IEnumerable<Sector>> GetByLocationIdAsync(int locationId);
    Task<bool> ExistsWithNameInLocationAsync(string name, int locationId);
    Task<bool> IsLockedAsync(int sectorId);


}