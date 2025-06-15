using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class EventSectorRepository : GenericRepository<EventSector>, IEventSectorRepository
{
    public EventSectorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<EventSector>> GetByEventIdAsync(int eventId)
    {
        return await _context.EventSectors
            .Include(es => es.Subsector)
            .ThenInclude(ss => ss.Sector) 
            .Where(es => es.EventId == eventId)
            .ToListAsync();
    }
    
    public new async Task<EventSector?> GetByIdAsync(int id)
    {
        return await _context.EventSectors
            .Include(e => e.Subsector)
            .FirstOrDefaultAsync(e => e.EventSectorId == id);
    }
    
    public async Task<EventSector?> GetByIdWithSubsectorAsync(int id)
    {
        return await _context.EventSectors
            .Include(e => e.Event)
            .Include(e => e.Subsector)
            .ThenInclude(s => s.Sector)
            .Include(e => e.Event.Location)
            .FirstOrDefaultAsync(e => e.EventSectorId == id);
    }
    
    public async Task UpdateAsync(EventSector sector)
    {
        _context.EventSectors.Update(sector);
        await _context.SaveChangesAsync();
    }
    public async Task<EventSector?> GetByEventIdAndSubsectorIdAsync(int eventId, int subsectorId)
    {
        return await _context.EventSectors
            .FirstOrDefaultAsync(es => es.EventId == eventId && es.SubsectorId == subsectorId);
    }



}