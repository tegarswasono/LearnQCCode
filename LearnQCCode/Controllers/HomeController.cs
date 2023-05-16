using LearnQCCode.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace LearnQCCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string TicketId = Guid.NewGuid().ToString();
            //write your code here
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(TicketId, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = BitmapToByteArray(QrBitmap);
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeUri = QrUri;
            ViewBag.TicketId = TicketId;
            return View();
        }

        public IActionResult Privacy()
        {
            SendEmail();
            return View();
        }

        private void SendEmail()
        {
            string SMTPServer = "smtp.googlemail.com";
            int SMTPPort = 587;
            string SMTPUser = "";
            string SMTPPassword = "";
            bool SMTPIsUseSSL = true;
            string emailDestination = "tegar.s@weefer.co.id";

            SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
            client.Credentials = new System.Net.NetworkCredential(SMTPUser, SMTPPassword);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = SMTPIsUseSSL;

            var path = Path.Combine(@"wwwroot\images\" + "Logo1.png");
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(SMTPUser, "no-reply");
                    mailMessage.To.Add(emailDestination);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Subject = "Cruise Confirmation - " + "Hallo Tegar";
                    mailMessage.AlternateViews.Add(GetEmbeddedImage(path));
                    client.Send(mailMessage);
                }
            }
        }
        private AlternateView GetEmbeddedImage(String filePath)
        {
            LinkedResource res = new LinkedResource(filePath, new ContentType("image/png"));
            LinkedResource res2 = new LinkedResource(GenerateBarcode(Guid.NewGuid()), new ContentType("image/Bmp"));

            res.ContentId = Guid.NewGuid().ToString();
            res2.ContentId = Guid.NewGuid().ToString();
            string body = "" +
                "<img src='cid:" + res.ContentId + @"' height='40' /><br/><br/>" +
                "Dear Sir/Madam " + "Bpk Tegar" + ",<br/><br/>" +
                "Below are your booking confirmation details:<br/>" +
                "<table>" +
                "<tr><td style='width: 120px;'>Vessel</td><td>: " + "dailyBookingPassenger.DailyBooking.Vessel.Name" + "</td></tr>" +
                "<tr><td>Departure Date</td><td>: " + "BookingDate" + " " + "Timespan" + "</td></tr>" +
                "<tr><td>Passenger Name</td><td>: " + "Passenger.Name" + "</td></tr>" +
                "<tr><td>Ic</td><td>: " + "Passenger.ICNo" + "</td></tr>" +
                "<tr><td>Booked By</td><td>: " + "CreatedUser" + "</td></tr>" +
                "</table>" +
                "<br/>Below is your QR Code:<br/>" +
                "<img src='cid:" + res2.ContentId + @"' height='200' /><br/>" +
                "addText" + "<br/><br/>" +
                "Thank you" +
                "";
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            alternateView.LinkedResources.Add(res2);
            return alternateView;
        }
        private Stream GenerateBarcode(Guid ticketId)
        {
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(ticketId.ToString(), QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = BitmapToByteArray(QrBitmap);
            MemoryStream logo = new MemoryStream(BitmapArray);
            return logo;
        }
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
