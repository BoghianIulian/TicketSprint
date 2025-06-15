using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Ticket>> GetByEventSectorIdAsync(int eventSectorId)
    {
        return await _context.Tickets
            .Where(t => t.EventSectorId == eventSectorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByEmailAsync(string email)
    {
        return await _context.Tickets
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Event)
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Subsector)
            .ThenInclude(ss => ss.Sector)
            .Where(t => t.Email == email)
            .ToListAsync();
    }
    
    public async Task<Ticket?> GetFullTicketModelAsync(int ticketId)
    {
        return await _context.Tickets
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Event)
            .ThenInclude(e => e.Participant1)
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Event)
            .ThenInclude(e => e.Participant2)
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Event)
            .ThenInclude(e => e.Location)
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Subsector)
            .ThenInclude(ss => ss.Sector) 
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }
    
    public async Task<Ticket?> GetByQRCodeAsync(string qrCode)
    {
        return await _context.Tickets
            .Include(t => t.EventSector)
            .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(t => t.QRCode == qrCode);
    }
    
    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }




}