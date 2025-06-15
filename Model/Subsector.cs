using System.Text.Json.Serialization;

namespace TicketSprint.Model;

public class Subsector
{
    public int SubsectorId { get; set; }
    public string SubsectorName { get; set; } 

    public int SectorId { get; set; }
    public Sector Sector { get; set; }

    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }

    [JsonIgnore]
    public ICollection<EventSector> EventSectors { get; set; } = new List<EventSector>();

    public int SeatsCount => Rows * SeatsPerRow;
}
