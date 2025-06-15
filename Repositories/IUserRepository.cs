using TicketSprint.Model;

namespace TicketSprint.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByEventIdAsync(int eventId);
    
    Task<User?> GetByIdAsync(int id);

    
}
