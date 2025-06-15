using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TicketSprint.Pages;

public class RegisterModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}