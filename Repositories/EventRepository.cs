using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Event>> GetByLocationIdAsync(int locationId)
    {
        return await _context.Events
            .Where(e => e.LocationId == locationId)
            .ToListAsync();
    }
    
    public async Task<bool> UpdateAsync(Event updatedEvent)
    {
        var existing = await _context.Events.FindAsync(updatedEvent.EventId);
        if (existing == null) return false;

        existing.EventName = updatedEvent.EventName;
        existing.EventDate = updatedEvent.EventDate;
        existing.Description = updatedEvent.Description;
        existing.ImageUrl = updatedEvent.ImageUrl ?? existing.ImageUrl;

        

        _context.Events.Update(existing);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<Participant?> GetParticipantByNameAndSport(string name, string sport)
    {
        return await _context.Participants.FirstOrDefaultAsync(p => p.Name == name && p.SportType == sport);
    }

    public async Task<Location?> GetLocationByName(string name)
    {
        return await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == name);
    }

    public async Task<bool> CheckConflict(DateTime date, int locationId)
    {
        return await _context.Events.AnyAsync(e =>
            e.LocationId == locationId &&
            e.EventDate.Date == date.Date);
    }

    public async Task<Event> CreateAsync(Event e)
    {
        _context.Events.Add(e);
        await _context.SaveChangesAsync();
        return e;
    }
    
    public async Task<FilterOptionsDTO> GetFilterOptionsAsync()
    {
        var sports = await _context.Events
            .Select(e => e.SportType)
            .Distinct()
            .ToListAsync();

        var cities = await _context.Locations
            .Select(l => l.City)
            .Distinct()
            .ToListAsync();

        var locations = await _context.Locations
            .Select(l => new LocationOptionDTO
            {
                LocationName = l.LocationName,
                City = l.City
            })
            .ToListAsync();

        return new FilterOptionsDTO
        {
            Sports = sports,
            Cities = cities,
            Locations = locations
        };
    }
    
    public async Task<List<SuggestedEventDTO>> GetEventsByParticipantsAsync(List<int> participantIds)
    {
        var participantSet = participantIds.ToHashSet();
        Console.WriteLine("Verific participanți: " + string.Join(", ", participantIds));

        var events = await _context.Events
            .Where(e => participantSet.Contains(e.Participant1Id) || participantSet.Contains(e.Participant2Id))
            .Include(e => e.Location)
            .Include(e => e.Participant1)
            .Include(e => e.Participant2)
            .ToListAsync();
        Console.WriteLine("Evenimente găsite: " + events.Count);

        return events.Select(e => new SuggestedEventDTO
        {
            EventId = e.EventId,
            EventName = e.EventName,
            SportType = e.SportType,
            EventDate = e.EventDate,
            LocationName = e.Location.LocationName,
            Participant1Name = e.Participant1.Name,
            Participant2Name = e.Participant2.Name,
            ImageUrl = e.ImageUrl,
            Description = e.Description,
            Participant1Id = e.Participant1Id,
            Participant2Id = e.Participant2Id
        }).ToList();
    }
    
    public async Task<bool> HasActiveFutureEventsBySubsectorIdAsync(int subsectorId)
    {
        return await _context.EventSectors
            .AnyAsync(es =>
                es.SubsectorId == subsectorId &&
                es.IsActive &&
                es.Event.EventDate > DateTime.Now);
    }
    
    public async Task<bool> HasFutureEventsByLocationIdAsync(int locationId)
    {
        return await _context.Events
            .AnyAsync(e => e.LocationId == locationId && e.EventDate > DateTime.Now);
    }




}