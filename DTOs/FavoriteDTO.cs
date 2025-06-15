namespace TicketSprint.DTOs;

public class FavoriteDTO
{
    public int UserId { get; set; }
    public string ParticipantName { get; set; }
    
    public int FavoriteId { get; set; }       
    
    public int ParticipantId { get; set; }
    
    public string SportType { get; set; }
    public string? ImageUrl { get; set; }
}