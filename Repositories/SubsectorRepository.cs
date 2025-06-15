using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class SubsectorRepository : GenericRepository<Subsector>, ISubsectorRepository
{
    public SubsectorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Subsector>> GetBySectorIdAsync(int sectorId)
    {
        return await _context.Subsectors
            .Include(s => s.Sector)
            .Where(s => s.SectorId == sectorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subsector>> GetByLocationIdAsync(int locationId)
    {
        return await _context.Subsectors
            .Include(s => s.Sector)
            .Where(s => s.Sector.LocationId == locationId)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalSeatsByLocationIdAsync(int locationId)
    {
        return await _context.Subsectors
            .Where(s => s.Sector.LocationId == locationId)
            .SumAsync(s => s.Rows * s.SeatsPerRow);
    }
    
    public async Task<bool> ExistsWithNameInSectorAsync(string name, int sectorId)
    {
        return await _context.Subsectors
            .AnyAsync(ss => ss.SubsectorName == name && ss.SectorId == sectorId);
    }
    
    public async Task<bool> IsLockedAsync(int subsectorId)
    {
        return await _context.EventSectors
            .Include(es => es.Event)
            .AnyAsync(es =>
                es.SubsectorId == subsectorId &&
                es.IsActive &&
                es.Event.EventDate >= DateTime.Now);
    }



}