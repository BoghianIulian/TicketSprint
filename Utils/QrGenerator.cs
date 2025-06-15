using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;


namespace TicketSprint.Utils;

public static class QrGenerator
{
    public static byte[] GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new BitmapByteQRCode(qrData);
        return qrCode.GetGraphic(20);
    }
}