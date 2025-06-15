using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TicketSprint.Pages.Shared;

public class _Navbar : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}