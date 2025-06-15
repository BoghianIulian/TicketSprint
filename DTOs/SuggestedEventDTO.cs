namespace TicketSprint.DTOs;

public class SuggestedEventDTO
{
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string SportType { get; set; }
    public DateTime EventDate { get; set; }
    public string LocationName { get; set; }
    public string Participant1Name { get; set; }
    public string Participant2Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }

    // 🔥 Adăugat special pentru logică de sugestii
    public int Participant1Id { get; set; }
    public int Participant2Id { get; set; }
}