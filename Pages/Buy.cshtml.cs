using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.DTOs;
using TicketSprint.Services;

namespace TicketSprint.Pages;

public class BuyModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IEventSectorService _eventSectorService;
    private readonly ISectorService _sectorService;
    private readonly ILocationService _locationService;

    public BuyModel(IEventService eventService, IEventSectorService eventSectorService, ISectorService sectorService, ILocationService locationService)
    {
        _eventService = eventService;
        _eventSectorService = eventSectorService;
        _sectorService = sectorService;
        _locationService = locationService;
    }

    public EventDTO Event { get; set; }
    
    
    public List<EventSectorDisplayDTO> DisplaySectors { get; set; } = new();
    public List<SectorDTO> Sectors { get; set; } = new(); // pentru dropdown filtru





    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetByIdAsync(id);
        if (Event == null)
        {
            return NotFound();
        }

        DisplaySectors = (await _eventSectorService.GetByEventIdAsync(id))
            .Where(es => es.IsActive)
            .Select(es => new EventSectorDisplayDTO
            {
                EventSectorId = es.EventSectorId,
                SubsectorName = es.Subsector.SubsectorName,
                SectorId = es.Subsector.SectorId,
                SectorName = es.Subsector.Sector.SectorName,
                AvailableSeats = es.AvailableSeats,
                Price = es.Price,
                IsActive = es.IsActive
            }).ToList();
        
        
        var location = await _locationService.GetByNameAsync(Event.LocationName);
        if (location == null)
        {
            return NotFound("Locația nu a fost găsită.");
        }
        Sectors = (await _sectorService.GetByLocationIdAsync(location.LocationId))
            .Select(s => new SectorDTO
            {
                SectorId = s.SectorId,
                SectorName = s.SectorName
            }).ToList();


        return Page();



    }
}