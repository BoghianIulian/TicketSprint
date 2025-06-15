namespace TicketSprint.DTOs;

public class EventSectorDisplayDTO
{
    public int EventSectorId { get; set; }
    public string SubsectorName { get; set; }
    public int SectorId { get; set; }
    public string SectorName { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}