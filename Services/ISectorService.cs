using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Services;

public interface ISectorService
{
    Task<IEnumerable<Sector>> GetAllAsync();
    Task<Sector?> GetByIdAsync(int id);
    Task<IEnumerable<Sector>> GetByLocationIdAsync(int locationId);
    Task<Sector> CreateAsync(Sector sector);
    Task<bool> UpdateAsync(int id, UpdateSectorDTO sector);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsLockedAsync(int sectorId);

}