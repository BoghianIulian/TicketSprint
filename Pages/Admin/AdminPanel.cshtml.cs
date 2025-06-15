using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.Model;
using TicketSprint.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TicketSprint.DTOs;

namespace TicketSprint.Pages.Admin
{
    public class AdminPanelModel : PageModel
    {
        private readonly ILocationService _locationService;
        private readonly ISectorService _sectorService;
        private readonly ISubsectorService _subsectorService;
        private readonly IParticipantService _participantService;
        private readonly IEventService _eventService;
        private readonly IEventSectorService _eventSectorService;

        public List<Location> Locations { get; set; } = new();
        public Dictionary<int, List<Sector>> SectorsByLocation { get; set; } = new();
        public Dictionary<int, List<Subsector>> SubsectorBySector { get; set; } = new();

        public List<Participant> Participants { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<EventSectorDisplayDTO> DisplaySectors { get; set; } = new();
        public Dictionary<int, List<EventSectorDisplayDTO>> EventSectorsByEvent { get; set; } = new();

        public AdminPanelModel(
            ILocationService locationService,
            ISectorService sectorService,
            ISubsectorService subsectorService,
            IParticipantService participantService,
            IEventService eventService,
            IEventSectorService eventSectorService)
        {
            _locationService = locationService;
            _sectorService = sectorService;
            _subsectorService = subsectorService;
            _participantService = participantService;
            _eventService = eventService;
            _eventSectorService = eventSectorService;
        }

        public async Task OnGetAsync()
{
    Locations = (await _locationService.GetAllAsync()).ToList();
    Participants = (await _participantService.GetAllAsync()).ToList();
    Events = (await _eventService.GetAllAsync()).ToList();

    foreach (var location in Locations)
    {
        var sectors = await _sectorService.GetByLocationIdAsync(location.LocationId);
        SectorsByLocation[location.LocationId] = sectors.ToList();

        foreach (var sector in sectors)
        {
            var subsectoareDto = await _subsectorService.GetBySectorIdAsync(sector.SectorId);
            var subsectoare = subsectoareDto.Select(dto => new Subsector
            {
                SubsectorId = dto.SubsectorId,
                SectorId = dto.SectorId,
                SubsectorName = dto.SubsectorName,
                Rows = dto.Rows,
                SeatsPerRow = dto.SeatsPerRow
            }).ToList();

            SubsectorBySector[sector.SectorId] = subsectoare;
        }
    }

    // Aici o singură dată toate EventSector
    var allEventSectors = await _eventSectorService.GetAllAsync();

    DisplaySectors = allEventSectors
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

    foreach (var ev in Events)
    {
        var sectoare = allEventSectors
            .Where(es => es.EventId == ev.EventId && es.IsActive)
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

        EventSectorsByEvent[ev.EventId] = sectoare;
    }
}

    }
}
