namespace TicketSprint.DTOs;

public class UpdateSubsectorDTO
{
    public string SubsectorName { get; set; } // ex: "T1A"

    public int SectorId { get; set; }
    
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}