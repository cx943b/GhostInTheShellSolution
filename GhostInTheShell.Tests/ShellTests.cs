using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Text;

namespace GhostInTheShell.Tests
{
    public class CharacterInfo : ICharacter
    {
        public string ShellName { get; init; }
        public Size ShellSize { get; init; }

        public CharacterInfo(string shellName, Size shellSize)
        {
            if(String.IsNullOrEmpty(shellName))
                throw new ArgumentNullException(nameof(shellName));
            if(ShellSize.IsEmpty)
                throw new ArgumentNullException(nameof(shellSize));

            ShellSize = shellSize;
            ShellName = shellName;
        }
    }


    [TestClass]
    public class ShellTests
    {
        IDictionary<string, Size> _dicCharInfo = new Dictionary<string, Size>()
        {
            { "Kaori", new Size(650, 1250) },
            { "Fumino", new Size(650, 1200) }
        };

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
        public async Task TableTest()
        {
            var logger = LoggerMockFactory.CreateLogger<ShellModelRemoteFactory>();
            HttpClient client = new HttpClient();

            ShellModelRemoteFactory modelFac = new ShellModelRemoteFactory(logger, _config, client);

            bool isTableReady = await modelFac.InitializeAsync(ShellName);
            Assert.IsTrue(isTableReady);
        }

        [TestMethod]
        public async Task MaterialTest()
        {
            HttpClient client = new HttpClient();

            ShellModelRemoteFactory modelFac = new ShellModelRemoteFactory(LoggerMockFactory.CreateLogger<ShellModelRemoteFactory>(), _config, client);
            bool isTableReady = await modelFac.InitializeAsync(ShellName);
            
            Assert.IsTrue(isTableReady);

            string stockingLabel = modelFac.GetLabels(ShellPartType.Stocking).ElementAt(2);
            var stockingModel = modelFac.GetModels(ShellPartType.Stocking, stockingLabel).First();
            
            Assert.IsNotNull(stockingModel);

            string clothLabel = modelFac.GetLabels(ShellPartType.Cloth).ElementAt(8);
            var clothModel = modelFac.GetModels(ShellPartType.Cloth, clothLabel).First();

            Assert.IsNotNull(clothModel);

            ShellMaterialRemoteFactory matFac = new ShellMaterialRemoteFactory(LoggerMockFactory.CreateLogger<ShellMaterialRemoteFactory>(), _config, client);
            bool isStockingMaterialReady = await matFac.LoadMaterial(ShellName, stockingModel);
            bool isClothMaterialReady = await matFac.LoadMaterial(ShellName, clothModel);

            Assert.IsTrue(isStockingMaterialReady && isClothMaterialReady);
            
            var orderedMaterials = Enumerable.Empty<IMaterialModel>()
                .Concat(stockingModel.GetMaterials())
                .Concat(clothModel.GetMaterials())
                .OrderBy(m => m.MainIndex)
                .ThenBy(m => m.SubIndex);

            MemoryStream? matStream = null;
            FileStream? fs = null;

            try
            {
                matStream = matFac.Overlap(orderedMaterials, _dicCharInfo[ShellName]);
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
 