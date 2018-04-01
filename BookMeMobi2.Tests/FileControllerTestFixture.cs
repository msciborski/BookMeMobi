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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookMeMobi2.Tests
{
    public class FileControllerTestFixture : IDisposable
    {
        public ILogger<FilesController> Logger { get; private set; }
        public IMapper Mapper { get; private set; }

        public FileControllerTestFixture()
        {
            var loggerMock = new Mock<ILogger<FilesController>>();
            Logger = loggerMock.Object;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            Mapper = config.CreateMapper();
        }
        public void Dispose()
        {
        }
    }
}
