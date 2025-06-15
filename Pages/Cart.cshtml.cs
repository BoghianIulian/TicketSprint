using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.DTOs;
using TicketSprint.Services;

namespace TicketSprint.Pages;

public class CartModel : PageModel
{
    public string? UserId { get; private set; }
    public string? CartId { get; private set; }
    
    private readonly IUserService _userService;
    public List<TemporaryReservation> Items { get; private set; } = new();

    public CartModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task OnGet()
    {
        // 1. Obține userId dacă e logat
        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. Încearcă să ia cartId din query sau cookie (depinde cum îl gestionezi)
        CartId = Request.Query["cartId"].FirstOrDefault()
                 ?? Request.Cookies["cartId"];

        // 3. Obține rezervările din store
        if (!string.IsNullOrEmpty(UserId) && int.TryParse(UserId, out var numericId))
        {
            Items = TemporaryReservationStore.GetUserReservations(UserId);
            var user = await _userService.GetByIdAsync(numericId);
            ViewData["UserFirstName"] = user?.FirstName;
            ViewData["UserLastName"] = user?.LastName;
            ViewData["UserEmail"] = user?.Email;
            ViewData["UserAge"] = user?.Age;
        }
        else if (!string.IsNullOrEmpty(CartId))
        {
            Items = TemporaryReservationStore.GetCartReservations(CartId);
        }
    }
}