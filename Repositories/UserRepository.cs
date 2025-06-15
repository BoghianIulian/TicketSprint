using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }
    
    public async Task<IEnumerable<User>> GetUsersByEventIdAsync(int eventId)
    {
        // 1. Căutăm toate emailurile distincte de la biletele acelui eveniment
        var emails = await _context.Tickets
            .Where(t => t.EventSector.EventId == eventId)
            .Select(t => t.Email)
            .Distinct()
            .ToListAsync();

        // 2. Luăm userii care au aceste emailuri (dacă există)
        return await _context.Users
            .Where(u => emails.Contains(u.Email))
            .ToListAsync();
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    
    
}
