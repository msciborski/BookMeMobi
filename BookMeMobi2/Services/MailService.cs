using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BookMeMobi2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookMeMobi2.Services
{
    public class MailService : IMailService
    {
        private readonly SMTPSettings _smtpSettings;
        private readonly ILogger<MailService> _logger;
        public MailService(IOptions<SMTPSettings> smtpSettings, ILogger<MailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendMailAsync(string To, string subject, Attachment attachment)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = false;
            message.From = new MailAddress(_smtpSettings.SMTPUserName);
            message.To.Add(new MailAddress(To));
            message.Subject = subject;
            message.Attachments.Add(attachment);

            using (SmtpClient client = new SmtpClient(_smtpSettings.SMTPHost, _smtpSettings.SMTPPort))
            {
                client.Credentials = new NetworkCredential(_smtpSettings.SMTPUserName, _smtpSettings.SMTPPassword);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Mail sent from {_smtpSettings.SMTPUserName} to {To}.");
            }
        }
    }
}
