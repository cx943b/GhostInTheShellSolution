using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

            string clothLabel = "반바지긴소매사복";
            bool isClothChanged = await charSvc.ChangeCloth(clothLabel);
            Assert.IsTrue(isClothChanged);

            string underwearLabel = "04(下着)ﾌﾞﾗ0K水";
            bool isUnderwearChanged = await charSvc.ChangeUnderwear(underwearLabel);
            Assert.IsTrue(isUnderwearChanged);

            string stockingLabel = "[ｽﾄｯｷﾝｸﾞ] 01茶";
            bool isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.Stocking, stockingLabel);
            Assert.IsTrue(isAccessoryAdded);

            var orderedMaterials = charSvc.GetMaterials()
                .OrderBy(m => m.MainIndex)
                .ThenBy(m => m.SubIndex);

            MemoryStream? matStream = null;
            FileStream? fs = null;

            try
            {
                matStream = matFac.Overlap(orderedMaterials, charSvc.ShellSize);
                Assert.IsNotNull(matStream);

                fs = new FileStream("Test.png", FileMode.Create, FileAccess.Write);
                matStream.Position = 0;
                matStream.CopyTo(fs);
            }
            finally
            {
                fs?.Dispose();
                matStream?.Dispose();
            }
        }
    }
}
