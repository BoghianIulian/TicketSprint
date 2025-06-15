namespace TicketSprint.DTOs;

public class TicketWithRoleDTO
{
    public int TicketId { get; set; }
    public int Row { get; set; }
    public int Seat { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } // "Admin" sau "Client"
}
