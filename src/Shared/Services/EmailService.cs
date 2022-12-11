using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Shared.Interfaces;
using Shared.Settings;

namespace Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<AppSettings> _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_appSettings.Value.SmtpConfig.From);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient(_appSettings.Value.SmtpConfig.Host, _appSettings.Value.SmtpConfig.Port))
            {
                smtp.Credentials = new NetworkCredential(_appSettings.Value.SmtpConfig.Username, _appSettings.Value.SmtpConfig.Password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
        }
    }
}