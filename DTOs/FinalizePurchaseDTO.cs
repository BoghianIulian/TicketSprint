namespace TicketSprint.DTOs;

public class FinalizePurchaseDTO
{
    public string Email { get; set; }
    public List<int> TicketIds { get; set; }
}
