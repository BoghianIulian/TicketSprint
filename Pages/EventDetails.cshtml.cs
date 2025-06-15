using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;

namespace TicketSprint.Pages;

public class EventDetailsModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IEventSectorService _eventSectorService;

    public EventDTO Event { get; set; }
    public List<EventSector> EventSectors { get; set; }
    
    public List<EventDTO> RelatedEvents { get; set; } = new();


    public EventDetailsModel(IEventService eventService, IEventSectorService eventSectorService)
    {
        _eventService = eventService;
        _eventSectorService = eventSectorService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        
        Event = await _eventService.GetByIdAsync(id);
        if (Event == null)
        {
            return NotFound();
        }
        
        EventSectors = (await _eventSectorService.GetByEventIdAsync(id)).ToList();
        
        
        var allEvents = await _eventService.GetAllWithDetailsAsync();
        RelatedEvents = allEvents
            .Where(e =>
                e.EventId != Event.EventId && (
                    e.Participant1Name == Event.Participant1Name ||
                    e.Participant2Name == Event.Participant2Name ||
                    e.Participant1Name == Event.Participant2Name ||
                    e.Participant2Name == Event.Participant1Name))
            .Take(6) 
            .ToList();

        


        return Page();
    }
}
