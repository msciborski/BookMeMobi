using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BookMeMobi2.SendGrid.Models
{
    public class SendGridMessage
    {
        public List<Personalization> Personalizations;
        public List<Content> Content;
        public List<Attachment> Attachments;
        public EmailAddress From { get; set; }
        public void AddPersonalization(string toEmail, string subject)
        {
            if (Personalizations == null)
            {
                Personalizations = new List<Personalization>();
            }
            if (String.IsNullOrEmpty(toEmail))
            {
                throw new Exception("Email can't be null.");
            }

            var personalization = new Personalization()
            {
                To = new List<EmailAddress>()
                {
                    new EmailAddress(){Email = toEmail}
                },
                Subject = subject
            };

            Personalizations.Add(personalization);

        }

        public void AddContent(string value, string type = "text/plain")
        {
            if (Content == null)
            {
                Content = new List<Content>();
            }
            var content = new Content()
            {
                Type = type,
                Value = value
            };
            Content.Add(content);
        }

        public async Task AddAttchament(string fileName, Stream contentStream, string type = null, string disposition = null,
            string contentId = null)
        {
            if (Attachments == null)
            {
                Attachments = new List<Attachment>();
            }

            if (contentStream == null || !contentStream.CanRead)
            {
                throw new Exception("Can read from stream");
            }

            var contentLength = Convert.ToInt32(contentStream.Length);
            var streamBytes = new byte[contentLength];
            await contentStream.ReadAsync(streamBytes, 0, contentLength);

            var base64Content = Convert.ToBase64String(streamBytes);

            var attachment = new Attachment()
            {
                Content = base64Content,
                ContentId = contentId,
                Disposition = disposition,
                FileName = fileName
            };
            Attachments.Add(attachment);
        }
    }
}
