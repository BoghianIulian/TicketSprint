namespace TicketSprint.DTOs;

public class CreateTicketDTO
{
    public int EventSectorId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? Age { get; set; }
    public string Email { get; set; }

    public int Row { get; set; }
    public int Seat { get; set; }
}