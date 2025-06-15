namespace TicketSprint.DTOs;

public class CartTicketDTO
{
    public string EventName { get; set; }
    public string LocationName { get; set; }
    public DateTime EventDate { get; set; }
    public string SubsectorName { get; set; }
    public string SectorName { get; set; }
    public int Row { get; set; }
    public int Seat { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? UserId { get; set; }
    public string? CartId { get; set; }
}