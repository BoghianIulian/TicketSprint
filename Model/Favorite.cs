namespace TicketSprint.Model;

public class Favorite
{
    public int FavoriteId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; }
}