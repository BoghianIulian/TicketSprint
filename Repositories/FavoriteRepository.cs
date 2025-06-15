using Microsoft.EntityFrameworkCore;
using TicketSprint.Data;
using TicketSprint.Model;

namespace TicketSprint.Repositories;

public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
    {
        return await _context.Favorites
            .Include(f => f.Participant)
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsAsync(int userId, int participantId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ParticipantId == participantId);
    }
    
    public async Task<Favorite?> GetByUserAndParticipantAsync(int userId, int participantId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ParticipantId == participantId);
    }
    
    public async Task<Dictionary<int, int>> GetFrequentParticipantStatsAsync(string email)
    {
        var eventIds = await _context.Tickets
            .Where(t => t.Email == email)
            .Select(t => t.EventSector.EventId)
            .Distinct()
            .ToListAsync();

        if (!eventIds.Any()) return new();

        var participantPairs = await _context.Events
            .Where(e => eventIds.Contains(e.EventId))
            .Select(e => new { e.Participant1Id, e.Participant2Id }) 
            .ToListAsync();

        var countMap = new Dictionary<int, int>();

        foreach (var pair in participantPairs)
        {
            if (pair.Participant1Id != 0) 
            {
                if (!countMap.ContainsKey(pair.Participant1Id))
                    countMap[pair.Participant1Id] = 0;
                countMap[pair.Participant1Id]++;
            }

            if (pair.Participant2Id != 0)
            {
                if (!countMap.ContainsKey(pair.Participant2Id))
                    countMap[pair.Participant2Id] = 0;
                countMap[pair.Participant2Id]++;
            }
        }

        return countMap
            .Where(kv => kv.Value >= 2)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }


    
    


}