using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TicketSprint.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }




    }
}