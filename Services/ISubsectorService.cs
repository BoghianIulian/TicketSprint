using TicketSprint.DTOs;

namespace TicketSprint.Services;

public interface ISubsectorService
{
    Task<IEnumerable<SubsectorDTO>> GetAllAsync();
    Task<SubsectorDTO?> GetByIdAsync(int id);
    Task<IEnumerable<SubsectorDTO>> GetBySectorIdAsync(int sectorId);
    Task<IEnumerable<SubsectorDTO>> GetByLocationIdAsync(int locationId);
    Task<SubsectorDTO> CreateAsync(SubsectorDTO dto);
    Task<bool> UpdateAsync(int id, UpdateSubsectorDTO dto);
    Task<bool> DeleteAsync(int id);
    Task<int> GetUsedSeatsForLocationAsync(int locationId);
    Task<bool> IsLockedAsync(int subsectorId);


}