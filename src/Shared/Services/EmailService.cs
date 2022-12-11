using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfig _smtpConfig;

        public EmailService(IConfiguration configuration)
        {
            _smtpConfig = configuration.GetSection("SmtpConfig").Get<SmtpConfig>()
                                    ?? throw new ArgumentNullException("SmtpConfig");
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_smtpConfig.From);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port))
            {
                smtp.Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }           
        }
    }
}