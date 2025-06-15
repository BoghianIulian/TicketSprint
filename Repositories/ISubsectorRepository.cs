using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface ISubsectorRepository : IGenericRepository<Subsector>
{
    Task<IEnumerable<Subsector>> GetBySectorIdAsync(int sectorId);
    Task<IEnumerable<Subsector>> GetByLocationIdAsync(int locationId);
    Task<int> GetTotalSeatsByLocationIdAsync(int locationId);
    Task<bool> ExistsWithNameInSectorAsync(string name, int sectorId);
    Task<bool> IsLockedAsync(int subsectorId);


    
}