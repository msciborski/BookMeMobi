using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.Options;
using BookMeMobi2.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BookMeMobi2.Tests
{
    public class MailServiceTest : IClassFixture<MailServiceTestFixture<Startup>>
    {
        private readonly IOptions<SMTPSettings> _smtpSettings;
        private readonly ILogger<MailService> _logger;
        public MailServiceTest(MailServiceTestFixture<Startup> fixture)
        {
            _smtpSettings =
                (IOptions<SMTPSettings>)fixture.Server.Host.Services.GetService(typeof(IOptions<SMTPSettings>));
            _logger = new Mock<ILogger<MailService>>().Object;
        }

        [Fact]
        public async Task SendEmailSuccess()
        {
            //Assert
            var mailService = new MailService(_smtpSettings, _logger);
            
            await mailService.SendMailAsync("norbini12@gmail.com", "test", new Attachment(new MemoryStream(), "test.mobi"));
        }
    }
}
