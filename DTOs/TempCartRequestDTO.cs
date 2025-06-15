namespace TicketSprint.DTOs;

public class TempCartRequestDTO
{
    public int EventSectorId { get; set; }
    public List<SeatDTO> Seats { get; set; }
    public string? UserId { get; set; }  // dacă e logat
    public string? CartId { get; set; } 
}