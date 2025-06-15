namespace TicketSprint.Model;

public class Event
{
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string SportType { get; set; }
    public DateTime EventDate { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; }
    
    public int Participant1Id { get; set; }
    public Participant Participant1 { get; set; }

    public int Participant2Id { get; set; }
    public Participant Participant2 { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }

    public ICollection<EventSector> EventSectors { get; set; }
    
}