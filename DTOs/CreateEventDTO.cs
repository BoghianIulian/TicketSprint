namespace TicketSprint.Model;

public class CreateEventDTO
{
    public string EventName { get; set; }
    public string SportType { get; set; }
    public DateTime EventDate { get; set; }

    public string LocationName { get; set; }

    public string Participant1Name { get; set; }
    public string Participant2Name { get; set; }
    public IFormFile? ImageUrl { get; set; }
    
    public string? Description { get; set; }
}