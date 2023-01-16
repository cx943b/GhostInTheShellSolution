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
    public class CharacterTests
    {
        const string ShellName = "Kaori";//"Fumino";
        IConfiguration _config;

        [TestInitialize]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                   .AddJsonFile("G:\\SosoProjects\\GhostInTheShellSolution\\GhostInTheShell\\AppSettings.json")
                   .Build();
        }

        [TestMethod]
        public async Task CharacterInitTest()
        {
            HttpClient client = new HttpClient();

            ShellModelFactory modelFac = new ShellModelFactory(LoggerMockFactory.CreateLogger<ShellModelFactory>(), _config, client);
            ShellMaterialFactory matFac = new ShellMaterialFactory(LoggerMockFactory.CreateLogger<ShellMaterialFactory>(), _config, client);

            CharacterService charSvc = new CharacterService(LoggerMockFactory.CreateLogger<CharacterService>(), _config, modelFac, matFac, client);
            bool isCharServiceReady = await charSvc.InitializeAsync(ShellName);

            Assert.IsTrue(isCharServiceReady);
        }
    }
}
