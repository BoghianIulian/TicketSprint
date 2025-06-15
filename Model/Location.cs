using System.Text.Json.Serialization;

namespace TicketSprint.Model;

public class Location
{
    public int LocationId { get; set; }
    public string LocationName { get; set; }
    public string LocationType { get; set; }
    
    public string City { get; set; }
    
    public int Capacity { get; set; }

    [JsonIgnore]
    public ICollection<Event> Events { get; set; }
    [JsonIgnore]
    public ICollection<Sector> Sectors { get; set; }
}