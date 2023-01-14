using GhostInTheShell.Modules.Shell;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class ShellTests
    {
        IConfiguration _config;

        [TestInitialize]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                   .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("G:\\SosoProjects\\GhostInTheShellSolution\\GhostInTheShell\\AppSettings.json")
                   .Build();
        }


        [TestMethod]
        public async Task TableTest()
        {
            var logger = LoggerMockFactory.CreateLogger<ShellModelFactory>();
            HttpClient client = new HttpClient();

            ShellModelFactory modelFac = new ShellModelFactory(logger, _config, client);

            bool isTableReady = await modelFac.InitializeAsync("Fumino");
            Assert.IsTrue(isTableReady);
        }
    }
}
 