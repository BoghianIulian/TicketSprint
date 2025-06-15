namespace TicketSprint.DTOs;

public class CreateParticipantDTO
{
    public string Name { get; set; }
    public string SportType { get; set; }
    
    public IFormFile? Image { get; set; }

}