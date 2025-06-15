using TicketSprint.Model;

namespace TicketSprint.Repositories;


public interface ILocationRepository : IGenericRepository<Location>
{
    Task<Location?> GetByNameAsync(string name);
    
    Task<bool> DeleteIfNoFutureEventsAsync(int id);


}