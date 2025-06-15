using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class ParticipantService : IParticipantService
{
    private readonly IParticipantRepository _repository;

    public ParticipantService(IParticipantRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Participant>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Participant?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<Participant> CreateAsync(CreateParticipantDTO dto)
    {
        string? imagePath = null;

        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var savePath = Path.Combine("wwwroot", "images", "participants", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            imagePath = $"/images/participants/{fileName}";
        }

        var participant = new Participant
        {
            Name = dto.Name,
            SportType = dto.SportType,
            ImageUrl = imagePath
        };

        await _repository.AddAsync(participant);
        return participant;
    }
    
    public async Task<bool> UpdateAsync(int id, CreateParticipantDTO dto)
    {
        var participant = await _repository.GetByIdAsync(id);
        if (participant == null) return false;

        participant.Name = dto.Name;
        participant.SportType = dto.SportType;

        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var savePath = Path.Combine("wwwroot", "images", "participants", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            participant.ImageUrl = $"/images/participants/{fileName}";
        }

        await _repository.UpdateAsync(participant);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
    
    public async Task<IEnumerable<Participant>> GetNonFavoriteParticipantsAsync(HashSet<int> favoriteIds)
    {
        var all = await _repository.GetAllAsync();
        return all.Where(p => !favoriteIds.Contains(p.ParticipantId));
    }

    public async Task<IEnumerable<EventDTO>> GetEventsByParticipantIdAsync(int participantId)
    {
        var events = await _repository.GetEventsByParticipantIdAsync(participantId);
        return events.Select(e => new EventDTO
        {
            EventId = e.EventId,
            EventName = e.EventName,
            EventDate = e.EventDate,
            SportType = e.SportType,
            ImageUrl = e.ImageUrl,
            LocationName = e.Location?.LocationName,
            Participant1Name = e.Participant1?.Name,
            Participant2Name = e.Participant2?.Name
        });
    }

    
    public async Task<List<string>> GetDistinctSportsAsync()
    {
        return await _repository.GetDistinctSportsAsync();
    }
    
    public async Task<List<Participant>> GetBySportAsync(string sport)
    {
        return await _repository.GetBySportAsync(sport);
    }
    
    



    
    

}