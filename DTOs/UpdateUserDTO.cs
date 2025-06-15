namespace TicketSprint.DTOs;

public class UpdateUserDTO
{
    public int UserId { get; set; } 
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int Age { get; set; }
}