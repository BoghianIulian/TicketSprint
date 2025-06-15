using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly AppDbContext _context;

    public EventService(IEventRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
        
    }

    public async Task<IEnumerable<Event>> GetAllAsync() => await _repository.GetAllAsync();
    
    public async Task<IEnumerable<EventDTO>> GetAllWithDetailsAsync()
    {
        var events = await _repository.GetAllAsync();

        var locationMap = _context.Locations
            .ToDictionary(l => l.LocationId, l => l.LocationName);

        var participantMap = _context.Participants
            .ToDictionary(p => p.ParticipantId, p => p.Name);

        var result = events.Select(e => new EventDTO
        {
            EventId = e.EventId,
            EventName = e.EventName,
            SportType = e.SportType,
            EventDate = e.EventDate,
            LocationName = locationMap.ContainsKey(e.LocationId) ? locationMap[e.LocationId] : "",
            Participant1Name = participantMap.ContainsKey(e.Participant1Id) ? participantMap[e.Participant1Id] : "",
            Participant2Name = participantMap.ContainsKey(e.Participant2Id) ? participantMap[e.Participant2Id] : "",
            ImageUrl = e.ImageUrl,
            Description = e.Description
        });

        return result;
    }
    
    public async Task<IEnumerable<EventDTO>> GetAllBySportAsync(string sportType)
    {
        var events = await _repository.GetAllAsync();

        var locations = _context.Locations.ToDictionary(l => l.LocationId, l => l.LocationName);
        var participants = _context.Participants.ToDictionary(p => p.ParticipantId, p => p.Name);

        return events
            .Where(e => e.SportType.ToLower() == sportType.ToLower() && e.EventDate > DateTime.Now)
            .Select(e => new EventDTO
            {
                EventId = e.EventId,
                EventName = e.EventName,
                SportType = e.SportType,
                EventDate = e.EventDate,
                LocationName = locations.ContainsKey(e.LocationId) ? locations[e.LocationId] : "",
                Participant1Name = participants.ContainsKey(e.Participant1Id) ? participants[e.Participant1Id] : "",
                Participant2Name = participants.ContainsKey(e.Participant2Id) ? participants[e.Participant2Id] : "",
                ImageUrl = e.ImageUrl,
                Description = e.Description
            });
    }
    public async Task<EventDTO?> GetByIdAsync(int id)
    {
        
        var e = await _repository.GetByIdAsync(id);
        if (e == null) return null;

        
        var locationMap = _context.Locations
            .ToDictionary(l => l.LocationId, l => l.LocationName);

        var participantMap = _context.Participants
            .ToDictionary(p => p.ParticipantId, p => p.Name);
        
        var participantImageMap = _context.Participants
            .ToDictionary(p => p.ParticipantId, p => p.ImageUrl ?? "/images/default-team.jpg");

        

        return new EventDTO
        {
            EventId = e.EventId,
            EventName = e.EventName,
            SportType = e.SportType,
            EventDate = e.EventDate,
            LocationName = locationMap.ContainsKey(e.LocationId) ? locationMap[e.LocationId] : "",
            Participant1Name = participantMap.ContainsKey(e.Participant1Id) ? participantMap[e.Participant1Id] : "",
            Participant2Name = participantMap.ContainsKey(e.Participant2Id) ? participantMap[e.Participant2Id] : "",
            Participant1Image = participantImageMap.ContainsKey(e.Participant1Id) ? participantImageMap[e.Participant1Id] : "/images/default-team.jpg",
            Participant2Image = participantImageMap.ContainsKey(e.Participant2Id) ? participantImageMap[e.Participant2Id] : "/images/default-team.jpg",
            Participant1Id = e.Participant1Id,
            Participant2Id = e.Participant2Id,
            ImageUrl = e.ImageUrl,
            Description = e.Description
        };
    }


    public async Task<EventDTO> CreateAsync(CreateEventDTO dto)
    {
        var p1 = await _repository.GetParticipantByNameAndSport(dto.Participant1Name, dto.SportType);
        var p2 = await _repository.GetParticipantByNameAndSport(dto.Participant2Name, dto.SportType);
        var location = await _repository.GetLocationByName(dto.LocationName);

        if (p1 == null || p2 == null)
            throw new Exception("Unul dintre participanți nu există sau nu este din sportul selectat.");

        if (p1.ParticipantId == p2.ParticipantId)
            throw new Exception("Cei doi participanți trebuie să fie diferiți.");

        if (location == null)
            throw new Exception("Locația specificată nu a fost găsită.");

        bool conflict = await _repository.CheckConflict(dto.EventDate, location.LocationId);
        if (conflict)
            throw new Exception("Există deja un eveniment în această locație în ziua selectată.");

        //  Salvare imagine
        string? imagePath = null;
        if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var savePath = Path.Combine("wwwroot", "images", "events", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using var stream = new FileStream(savePath, FileMode.Create);
            await dto.ImageUrl.CopyToAsync(stream);

            imagePath = $"/images/events/{fileName}";
        }

        //  Creează Event pentru DB
        var newEvent = new Event
        {
            EventName = dto.EventName,
            SportType = dto.SportType,
            Participant1Id = p1.ParticipantId,
            Participant2Id = p2.ParticipantId,
            EventDate = dto.EventDate,
            LocationId = location.LocationId,
            Description = dto.Description,
            ImageUrl = imagePath
        };

        var saved = await _repository.CreateAsync(newEvent);

        //  Mapare în DTO pentru frontend
        return new EventDTO
        {
            EventId = saved.EventId,
            EventName = saved.EventName,
            SportType = saved.SportType,
            EventDate = saved.EventDate,
            LocationName = location.LocationName,
            Participant1Name = p1.Name,
            Participant2Name = p2.Name,
            Participant1Image = p1.ImageUrl,
            Participant2Image = p2.ImageUrl,
            ImageUrl = saved.ImageUrl,
            Description = saved.Description
        };
    }


    public async Task<bool> UpdateAsync(int id, CreateEventDTO dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == dto.LocationName);
        if (location == null) throw new Exception("Locația nu a fost găsită.");

        var p1 = await _context.Participants.FirstOrDefaultAsync(p => p.Name == dto.Participant1Name && p.SportType == dto.SportType);
        var p2 = await _context.Participants.FirstOrDefaultAsync(p => p.Name == dto.Participant2Name && p.SportType == dto.SportType);
        if (p1 == null || p2 == null) throw new Exception("Unul sau ambii participanți nu au fost găsiți.");

        string? imagePath = null;

        if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var savePath = Path.Combine("wwwroot", "images", "events", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await dto.ImageUrl.CopyToAsync(stream);
            }

            imagePath = $"/images/events/{fileName}";
        }

        existing.EventName = dto.EventName;
        existing.Description = dto.Description;
        existing.EventDate = dto.EventDate;

        if (imagePath != null)
            existing.ImageUrl = imagePath;

        // NU modificăm SportType, LocationId, Participant1Id, Participant2Id

        return await _repository.UpdateAsync(existing);
    }



    public async Task<bool> DeleteAsync(int id)
    {
        
        
        var existing = await _repository.GetByIdAsync(id);
        if (existing.EventDate > DateTime.Now)
            throw new InvalidOperationException("Nu poți șterge un eveniment viitor.");
        if (existing == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<Event>> GetByLocationIdAsync(int locationId)
        => await _repository.GetByLocationIdAsync(locationId);
    
    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .Include(e => e.Participant1)
            .Include(e => e.Participant2)
            .Include(e => e.Location)
            .ToListAsync();
    }
    
    public async Task<FilterOptionsDTO> GetFilterOptionsAsync()
    {
        return await _repository.GetFilterOptionsAsync();
    }


}