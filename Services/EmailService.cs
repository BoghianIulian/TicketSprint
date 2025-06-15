using MailKit.Net.Smtp;
using MimeKit;

namespace TicketSprint.Services;

public class EmailService
{
    private readonly string _from = "tickets.sprint@gmail.com";
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _smtpUser = "tickets.sprint@gmail.com";
    private readonly string _smtpPass = "aqpn kiqj mgef vrnl"; 

    public async Task SendTicketsEmail(string toEmail, List<(string FileName, byte[] Content)> attachments, string? firstName = null, string? lastName = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TicketSprint", _from));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Biletele tale de la TicketSprint";
        
        var fullName = (firstName + " " + lastName)?.Trim();

        var builder = new BodyBuilder
        {
            TextBody = $"Salut{(string.IsNullOrWhiteSpace(fullName) ? "" : " " + fullName)}!\n\n" +
                       "Îți mulțumim că ai ales TicketSprint pentru achiziția biletelor tale. Găsești în atașament biletele în format PDF, fiecare conținând un cod QR unic.\n\n" +
                       "Te rugăm să prezinți biletul (tipărit sau digital) la intrarea în locație, pentru validare.\n\n" +
                       "Îți dorim o experiență plăcută la meci!\n\n" +
                       "Echipa TicketSprint"
        };

        foreach (var (fileName, content) in attachments)
        {
            builder.Attachments.Add(fileName, content, new ContentType("application", "pdf"));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
