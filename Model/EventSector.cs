namespace TicketSprint.Model;

public class EventSector
{
    public int EventSectorId { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; }
    
    public int SubsectorId { get; set; }
    public Subsector Subsector { get; set; }
    

    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Ticket> Tickets { get; set; }
}