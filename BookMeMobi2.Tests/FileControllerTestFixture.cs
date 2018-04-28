using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using AutoMapper;
using BookMeMobi2.Controllers;
using BookMeMobi2.Helpers.Mappings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookMeMobi2.Tests
{
    public class FileControllerTestFixture<TStartup> : IDisposable where TStartup : class
     {
        public readonly TestServer Server;
        private readonly HttpClient _client;
        public IMapper Mapper { get; }
        public FileControllerTestFixture()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot($"C:\\Users\\msciborski\\Downloads\\source\\repos\\BookMeMobi2\\BookMeMobi2")
                .ConfigureAppConfiguration((context, buil) =>
                {
                    var env = context.HostingEnvironment;
                    buil.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                    buil.AddEnvironmentVariables();
                })
                .UseStartup<TStartup>();

            Server = new TestServer(builder);
            _client = new HttpClient();
        }
        public void Dispose()
        {
            Server.Dispose();
            _client.Dispose();
        }
    }
}
