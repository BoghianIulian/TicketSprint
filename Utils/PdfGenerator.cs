using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using TicketSprint.Model;

namespace TicketSprint.Utils;

public static class PdfGenerator
{
    private const string RootPath = "wwwroot"; 

    public static byte[] GenerateTicketPdf(Ticket ticket)
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        double pageWidth = page.Width;
        double y = 30;

        var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
        var fontHeader = new XFont("Arial", 14, XFontStyle.Bold);
        var fontBody = new XFont("Arial", 10, XFontStyle.Regular);
        var penLine = new XPen(XColors.LightGray, 1);

        
        if (!string.IsNullOrEmpty(ticket.EventSector.Event.ImageUrl))
        {
            try
            {
                var imagePath = Path.Combine(RootPath, ticket.EventSector.Event.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    var imgBytes = File.ReadAllBytes(imagePath);
                    using var imgStream = new MemoryStream(imgBytes);
                    var eventImage = XImage.FromStream(() => imgStream);

                    double imgWidth = 200;
                    double imgHeight = 100;
                    double imgX = (pageWidth - imgWidth) / 2;

                    gfx.DrawImage(eventImage, imgX, y, imgWidth, imgHeight);
                    y += imgHeight + 10;
                }
            }
            catch { }
        }

        gfx.DrawString(ticket.EventSector.Event.EventName.ToUpper(), fontTitle, XBrushes.Black,
            new XRect(0, y, pageWidth, 30), XStringFormats.TopCenter);
        y += 40;

        gfx.DrawLine(penLine, 40, y, pageWidth - 40, y);
        y += 15;

        // === 2. INFO EVENIMENT ===
        var p1 = ticket.EventSector.Event.Participant1;
        var p2 = ticket.EventSector.Event.Participant2;

        gfx.DrawString($"Data: {ticket.EventSector.Event.EventDate:dd.MM.yyyy}", fontBody, XBrushes.Black,
            new XRect(0, y, pageWidth, 20), XStringFormats.TopCenter);
        y += 20;

        gfx.DrawString($"Meci: {p1?.Name} vs {p2?.Name}", fontBody, XBrushes.Black,
            new XRect(0, y, pageWidth, 20), XStringFormats.TopCenter);
        y += 25;

        // Imagini participanți
        double partImgSize = 50;
        double partY = y;
        double p1X = pageWidth / 2 - partImgSize - 10;
        double p2X = pageWidth / 2 + 10;

        if (!string.IsNullOrEmpty(p1?.ImageUrl))
        {
            var path = Path.Combine(RootPath, p1.ImageUrl.TrimStart('/'));
            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                using var stream = new MemoryStream(bytes);
                var img = XImage.FromStream(() => stream);
                gfx.DrawImage(img, p1X, partY, partImgSize, partImgSize);
            }
        }

        if (!string.IsNullOrEmpty(p2?.ImageUrl))
        {
            var path = Path.Combine(RootPath, p2.ImageUrl.TrimStart('/'));
            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                using var stream = new MemoryStream(bytes);
                var img = XImage.FromStream(() => stream);
                gfx.DrawImage(img, p2X, partY, partImgSize, partImgSize);
            }
        }

        y += partImgSize + 30;
        gfx.DrawLine(penLine, 40, y, pageWidth - 40, y);
        y += 15;

        // === 3. DETALII BILET ===
        void DrawCenteredLine(string label, string value)
        {
            gfx.DrawString($"{label}: {value}", fontBody, XBrushes.Black,
                new XRect(0, y, pageWidth, 20), XStringFormats.TopCenter);
            y += 20;
        }

        DrawCenteredLine("Nume", $"{ticket.FirstName} {ticket.LastName}");
        DrawCenteredLine("Email", ticket.Email);
        var loc = ticket.EventSector.Event.Location;
        DrawCenteredLine("Locație", loc != null ? $"{loc.LocationName} ({loc.City})" : "–");

        var sector = ticket.EventSector.Subsector.Sector.SectorName ?? "–";
        var subsector = ticket.EventSector.Subsector.SubsectorName ?? "–";
        DrawCenteredLine("Sector", $"{sector} / {subsector}");

        DrawCenteredLine("Loc", $"Rând {ticket.Row}, Loc {ticket.Seat}");
        DrawCenteredLine("Data achiziției", ticket.PurchaseDate.ToString("dd.MM.yyyy"));

        y += 10;
        gfx.DrawLine(penLine, 40, y, pageWidth - 40, y);
        y += 15;

        // === 4. QR Code ===
        gfx.DrawString("Scanează codul QR la intrare:", fontHeader, XBrushes.Black,
            new XRect(0, y, pageWidth, 20), XStringFormats.TopCenter);
        y += 25;

        var scanUrl = $"http://localhost:5182/scan?code={ticket.QRCode}";
        var qrBytes = QrGenerator.GenerateQrCode(scanUrl);

        using var qrStream = new MemoryStream(qrBytes);
        var qrImage = XImage.FromStream(() => qrStream);
        double qrSize = 120;
        double qrX = (pageWidth - qrSize) / 2;
        gfx.DrawImage(qrImage, qrX, y, qrSize, qrSize);

        using var pdfStream = new MemoryStream();
        document.Save(pdfStream, false);
        return pdfStream.ToArray();
    }
}
