using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class LocationRepository : GenericRepository<Location>, ILocationRepository
{
    public LocationRepository(AppDbContext context) : base(context) { }
    
    public async Task<Location?> GetByNameAsync(string name)
    {
        return await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName == name);
    }
    
    public async Task<bool> DeleteIfNoFutureEventsAsync(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
            return false;

        var hasFutureEvents = await _context.Events
            .AnyAsync(e => e.LocationId == id && e.EventDate >= DateTime.Now);

        if (hasFutureEvents)
            throw new InvalidOperationException("Locația nu poate fi ștearsă deoarece are evenimente viitoare.");

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return true;
    }
}