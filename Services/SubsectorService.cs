using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class SubsectorService : ISubsectorService
{
    private readonly ISubsectorRepository _repository;
    private readonly ISectorRepository _sectorRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IEventRepository _eventRepository;

    public SubsectorService(ISubsectorRepository repository, ISectorRepository sectorRepository, ILocationRepository locationRepository, IEventRepository eventRepository)
    {
        _repository = repository;
        _sectorRepository = sectorRepository;
        _locationRepository = locationRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<SubsectorDTO>> GetAllAsync()
    {
        var data = await _repository.GetAllAsync();
        return data.Select(MapToDTO);
    }

    public async Task<SubsectorDTO?> GetByIdAsync(int id)
    {
        var s = await _repository.GetByIdAsync(id);
        return s == null ? null : MapToDTO(s);
    }

    public async Task<IEnumerable<SubsectorDTO>> GetBySectorIdAsync(int sectorId)
    {
        var data = await _repository.GetBySectorIdAsync(sectorId);
        return data.Select(MapToDTO);
    }

    public async Task<IEnumerable<SubsectorDTO>> GetByLocationIdAsync(int locationId)
    {
        var data = await _repository.GetByLocationIdAsync(locationId);
        return data.Select(MapToDTO);
    }

    public async Task<SubsectorDTO> CreateAsync(SubsectorDTO dto)
    {
        var sector1 = await _sectorRepository.GetByIdAsync(dto.SectorId);
        if (sector1 == null)
            throw new InvalidOperationException("Sectorul nu există.");

        var location = await _locationRepository.GetByIdAsync(sector1.LocationId);
        if (location == null)
            throw new InvalidOperationException("Locația nu există.");

        var usedSeats = await _repository.GetTotalSeatsByLocationIdAsync(location.LocationId);
        var newSeats = dto.Rows * dto.SeatsPerRow;

        if (usedSeats + newSeats > location.Capacity)
            throw new InvalidOperationException($"Capacitatea maximă a locației a fost depășită. Locuri disponibile: {location.Capacity - usedSeats}");

        var entity = new Subsector
        {
            SubsectorName = dto.SubsectorName,
            SectorId = dto.SectorId,
            Rows = dto.Rows,
            SeatsPerRow = dto.SeatsPerRow
        };

        await _repository.AddAsync(entity);

        // re-include sector name if needed
        var sector = entity.Sector ?? new Sector { SectorName = "N/A" };

        return new SubsectorDTO
        {
            SubsectorId = entity.SubsectorId,
            SubsectorName = entity.SubsectorName,
            SectorId = entity.SectorId,
            SectorName = sector.SectorName,
            Rows = entity.Rows,
            SeatsPerRow = entity.SeatsPerRow
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateSubsectorDTO dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;
        var duplicate = await _repository.GetBySectorIdAsync(dto.SectorId);
        if (duplicate.Any(ss => ss.SubsectorName == dto.SubsectorName && ss.SubsectorId != id))
            throw new InvalidOperationException("Există deja un alt subsector cu acest nume în același sector.");

        if ((existing.Rows != dto.Rows || existing.SeatsPerRow != dto.SeatsPerRow)
            && await _eventRepository.HasActiveFutureEventsBySubsectorIdAsync(id))
        {
            throw new InvalidOperationException("Nu poți modifica structura acestui subsector deoarece este asociat unui eveniment activ din viitor.");
        }

        existing.SubsectorName = dto.SubsectorName;
        existing.SectorId = dto.SectorId;
        existing.Rows = dto.Rows;
        existing.SeatsPerRow = dto.SeatsPerRow;

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        if (await _eventRepository.HasFutureEventsByLocationIdAsync(id))
        {
            throw new InvalidOperationException("Locația are evenimente viitoare și nu poate fi ștearsă.");
        }

        await _repository.DeleteAsync(id);
        return true;
    }
    public async Task<int> GetUsedSeatsForLocationAsync(int locationId)
    {
        return await _repository.GetTotalSeatsByLocationIdAsync(locationId);
    }
    
    public async Task<bool> IsLockedAsync(int subsectorId)
    {
        return await _repository.IsLockedAsync(subsectorId);
    }



    private SubsectorDTO MapToDTO(Subsector s)
    {
        return new SubsectorDTO
        {
            SubsectorId = s.SubsectorId,
            SubsectorName = s.SubsectorName,
            SectorId = s.SectorId,
            SectorName = s.Sector?.SectorName ?? "",
            Rows = s.Rows,
            SeatsPerRow = s.SeatsPerRow
        };
    }
}