using System.Text.Json.Serialization;

namespace TicketSprint.Model;

public class Sector
{
    public int SectorId { get; set; }
    public string SectorName { get; set; }
    public int LocationId { get; set; }

    [JsonIgnore]
    public Location Location { get; set; }
    

    [JsonIgnore]
    public ICollection<Subsector> Subsectors { get; set; } = new List<Subsector>();}