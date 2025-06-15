using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.Services;

namespace TicketSprint.Pages;

public class ScanModel : PageModel
{
    private readonly ITicketService _ticketService;

    public ScanModel(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [BindProperty(SupportsGet = true)]
    public string Code { get; set; }

    public string StatusMessage { get; set; }
    public bool Success { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Code))
        {
            StatusMessage = "❌ Codul QR este invalid sau lipsește.";
            return Page();
        }

        var ticket = await _ticketService.GetByQRCodeAsync(Code);
        if (ticket == null)
        {
            StatusMessage = "❌ Biletul nu a fost găsit.";
            return Page();
        }

        if (ticket.IsScanned)
        {
            StatusMessage = $"⚠️ Biletul a fost deja scanat pentru evenimentul: {ticket.EventSector.Event.EventName}";
            return Page();
        }

        ticket.IsScanned = true;
        await _ticketService.UpdateAsync(ticket);

        StatusMessage = $"✅ Bilet scanat cu succes pentru evenimentul: {ticket.EventSector.Event.EventName}";
        Success = true;
        return Page();
    }
}