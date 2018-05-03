using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.Options;
using BookMeMobi2.SendGrid.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BookMeMobi2.Services
{
    public class SendGridMailService : IMailService
    {
        private HttpClient _client;
        private readonly SendGridSettings _settings;
        private readonly JsonSerializerSettings _serializerSettings;


        public SendGridMailService(IOptions<SendGridSettings> settings)
        {
            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            _serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            _settings = settings.Value;

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        public async Task SendMailAsync(string To, string subject, string messageContent = null, Stream stream = null, string fileName = null)
        {
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.AddPersonalization(To, subject);

            var content = messageContent == null ? "" : messageContent;
            sendGridMessage.AddContent(content);

            if (stream != null && !String.IsNullOrEmpty(fileName))
            {
                await sendGridMessage.AddAttchament(fileName, stream);
            }

            var jsonRequest = JsonConvert.SerializeObject(sendGridMessage, _serializerSettings);

            var respone = await _client.PostAsync(_settings.SendUrl,
                new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception("Cant send mail.");
            }



        }
}
