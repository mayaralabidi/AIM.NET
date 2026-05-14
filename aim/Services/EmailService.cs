using System.Net;
using System.Net.Mail;

namespace aim.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendAsync(string toEmail, string toName,
                                     string subject, string htmlBody)
        {
            var section = _config.GetSection("EmailSettings");
            var host = section["SmtpHost"] ?? "";
            var port = int.Parse(section["SmtpPort"] ?? "587");
            var user = section["SmtpUser"] ?? "";
            var password = section["SmtpPassword"] ?? "";
            var fromName = section["FromName"] ?? "AIM";
            var fromEmail = section["FromEmail"] ?? user;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
                return; // email not configured — skip silently

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var from = new MailAddress(fromEmail, fromName);
            var to = new MailAddress(toEmail, toName);

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}