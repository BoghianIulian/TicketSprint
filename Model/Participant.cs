using System.Text.Json.Serialization;

namespace TicketSprint.Model;

public class Participant
{
    public int ParticipantId { get; set; }
    public string Name { get; set; }
    public string SportType { get; set; }
    
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public ICollection<Event> EventsAsParticipant1 { get; set; } = new List<Event>();
    [JsonIgnore]
    public ICollection<Event> EventsAsParticipant2 { get; set; } = new List<Event>();
}