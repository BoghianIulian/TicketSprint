namespace TicketSprint.DTOs;

public class CreateSubsectorDTO
{
    public string SubsectorName { get; set; } = string.Empty;
    public int SectorId { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}
