using TicketSprint.Model;

namespace TicketSprint.DTOs;

public class EventDetailsViewModel
{
    public EventDTO Event { get; set; }
    public List<EventSector> EventSectors { get; set; } = new();
}