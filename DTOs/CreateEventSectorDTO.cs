namespace TicketSprint.DTOs;

public class CreateEventSectorDTO
{
    public int EventId { get; set; }
    public int SubsectorId { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}