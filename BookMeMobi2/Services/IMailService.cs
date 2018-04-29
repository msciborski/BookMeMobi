using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BookMeMobi2.Services
{
    public interface IMailService
    {
        Task SendMailAsync(string To, string subject, Attachment attachment = null, string messageContent = null);
    }
}
