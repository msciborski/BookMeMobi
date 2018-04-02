using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace BookMeMobi2.Tests
{
    class UserServiceTestFixture<TStarup> : IDisposable where TStarup : class
    {
        public readonly TestServer Server;
        private readonly HttpClient _client;

        public UserServiceTestFixture()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot($"C:\\Users\\msciborski\\Downloads\\source\\repos\\BookMeMobi2\\BookMeMobi2")
                .UseStartup<TStarup>();
        }
        public void Dispose()
        {
            Server.Dispose();
            _client.Dispose();
        }
    }
}
