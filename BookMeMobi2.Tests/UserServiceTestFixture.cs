using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace BookMeMobi2.Tests
{
    public class UserServiceTestFixture<TStarup> : IDisposable where TStarup : class
    {
        public readonly TestServer Server;
        private readonly HttpClient _client;

        public UserServiceTestFixture()
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
                .UseStartup<TStarup>();

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
