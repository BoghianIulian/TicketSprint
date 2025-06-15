using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services; // sau unde ai definit _eventService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSprint.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly IFavoriteService _favoriteService;

        public IndexModel(IEventService eventService, IFavoriteService favoriteService)
        {
            _eventService = eventService;
            _favoriteService = favoriteService;
        }

        public List<SuggestedEventDTO> SuggestedEvents { get; set; } = new();

        public bool IsUserLoggedIn => User.Identity?.IsAuthenticated ?? false;
       

        public async Task OnGetAsync()
        {
            var hasFilters = Request.Query.Any();
            if (IsUserLoggedIn && !hasFilters)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    SuggestedEvents = await _favoriteService.GetSuggestedEventsForUserAsync(userId);
                }
                Console.WriteLine($"UserId din claims: {userIdClaim}");

            }
        }
    }
}