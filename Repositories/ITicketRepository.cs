using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetByEventSectorIdAsync(int eventSectorId);
    Task<IEnumerable<Ticket>> GetByEmailAsync(string email);
    Task<Ticket?> GetFullTicketModelAsync(int ticketId);
    Task<Ticket?> GetByQRCodeAsync(string qrCode);
    
    Task UpdateAsync(Ticket ticket);

}