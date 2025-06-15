namespace TicketSprint.DTOs;

public class SubsectorDTO
{
    public int SubsectorId { get; set; } // DOAR pentru citire
    public string SubsectorName { get; set; }
    public int SectorId { get; set; }
    public string SectorName { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}
