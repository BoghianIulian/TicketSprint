using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class ParticipantRepository : GenericRepository<Participant>, IParticipantRepository
{
    public ParticipantRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Event>> GetEventsByParticipantIdAsync(int participantId)
    {
        var asP1 = await _context.Events
            .Include(e => e.Participant1)
            .Include(e => e.Participant2)
            .Include(e => e.Location)
            .Where(e => e.Participant1Id == participantId)
            .ToListAsync();

        var asP2 = await _context.Events
            .Include(e => e.Participant1)
            .Include(e => e.Participant2)
            .Include(e => e.Location)
            .Where(e => e.Participant2Id == participantId)
            .ToListAsync();

        return asP1.Concat(asP2);
    }
    
    public async Task<List<string>> GetDistinctSportsAsync()
    {
        return await _context.Participants
            .Where(p => !string.IsNullOrWhiteSpace(p.SportType))
            .Select(p => p.SportType)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }
    
    public async Task<List<Participant>> GetBySportAsync(string sport)
    {
        return await _context.Participants
            .Where(p => p.SportType == sport)
            .ToListAsync();
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var participant = await _context.Participants.FindAsync(id);
        if (participant == null)
            return false;

        var hasUpcomingEvents = await _context.Events
            .AnyAsync(e =>
                (e.Participant1Id == id || e.Participant2Id == id) &&
                e.EventDate > DateTime.Now);

        if (hasUpcomingEvents)
            throw new InvalidOperationException("Nu poți șterge un participant care are evenimente viitoare.");

        var pastEvents = await _context.Events
            .Where(e =>
                (e.Participant1Id == id || e.Participant2Id == id) &&
                e.EventDate <= DateTime.Now)
            .ToListAsync();

        _context.Events.RemoveRange(pastEvents);
        _context.Participants.Remove(participant);

        await _context.SaveChangesAsync();
        return true;
    }



}