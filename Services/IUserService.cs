using TicketSprint.Model;

namespace TicketSprint.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    
    Task<User?> GetByIdAsync(int id);

}