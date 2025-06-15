namespace TicketSprint.Model;

public class Ticket
{
    public int TicketId { get; set; }

    public int EventSectorId { get; set; }
    public EventSector EventSector { get; set; }

    public DateTime PurchaseDate { get; set; }
    public string QRCode { get; set; }
    
    public int Row { get; set; }
    public int Seat { get; set; }
    
    public bool IsScanned { get; set; } = false;



    // Info utilizator guest (sau logat)
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? Age { get; set; }
    public string Email { get; set; }
}