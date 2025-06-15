using TicketSprint.DTOs;
using TicketSprint.Model;

namespace TicketSprint.Services;

public interface ITicketService
{
    Task<IEnumerable<TicketDTO>> GetAllAsync();
    Task<TicketDTO?> GetByIdAsync(int id);
    Task<IEnumerable<TicketDTO>> GetByEventSectorIdAsync(int eventSectorId);
    Task<IEnumerable<TicketDTO>> GetByEmailAsync(string email);
    Task<TicketDTO> CreateAsync(CreateTicketDTO dto);
    Task<bool> DeleteAsync(int id);
    Task<Ticket?> GetFullTicketModelAsync(int ticketId);
    Task<Ticket?> GetByQRCodeAsync(string qrCode);
    Task UpdateAsync(Ticket ticket);

    Task<List<TicketWithRoleDTO>> GetOccupiedSeatsWithRoleForAdmin(int eventSectorId);



}