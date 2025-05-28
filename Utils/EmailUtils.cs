using System.Net;
using System.Net.Mail;

namespace StudentCanvasApp.Helpers
{
    public static class EmailUtils
    {
        public static void SendEmail(string to, string subject, string body)
        {
            var fromAddress = new MailAddress("canvasstudent70@gmail.com", "Student Canvas App");
            var toAddress = new MailAddress(to);
            const string fromPassword = "bfzl jzpx vnnc kigz"; // App password from Gmail
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
