namespace TicketSprint.DTOs;

public class TemporaryReservation
{
    public string? UserId { get; set; }
    public string? CartId { get; set; }
    public int EventSectorId { get; set; }
    public int Row { get; set; }
    public int Seat { get; set; }
    public DateTime ExpiresAt { get; set; }
}