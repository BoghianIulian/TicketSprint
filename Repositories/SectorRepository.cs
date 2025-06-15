using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class SectorRepository : GenericRepository<Sector>, ISectorRepository
{
    public SectorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Sector>> GetByLocationIdAsync(int locationId)
    {
        return await _context.Sectors
            .Where(s => s.LocationId == locationId)
            .ToListAsync();
    }
    public async Task<bool> ExistsWithNameInLocationAsync(string name, int locationId)
    {
        return await _context.Sectors
            .AnyAsync(s => s.SectorName == name && s.LocationId == locationId);
    }
    public async Task<bool> IsLockedAsync(int sectorId)
    {
        var subsectorIds = await _context.Subsectors
            .Where(ss => ss.SectorId == sectorId)
            .Select(ss => ss.SubsectorId)
            .ToListAsync();

        return await _context.EventSectors
            .Include(es => es.Event)
            .AnyAsync(es =>
                subsectorIds.Contains(es.SubsectorId) &&
                es.IsActive &&
                es.Event.EventDate >= DateTime.Now);
    }
}