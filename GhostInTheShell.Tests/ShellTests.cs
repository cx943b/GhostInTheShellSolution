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
using System.Windows.Automation.Text;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class ShellTests
    {
        const string ShellName = "Fumino";
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

            bool isTableReady = await modelFac.InitializeAsync(ShellName);
            Assert.IsTrue(isTableReady);
        }

        [TestMethod]
        public async Task MaterialTest()
        {
            HttpClient client = new HttpClient();

            ShellModelFactory modelFac = new ShellModelFactory(LoggerMockFactory.CreateLogger<ShellModelFactory>(), _config, client);
            bool isTableReady = await modelFac.InitializeAsync(ShellName);
            
            Assert.IsTrue(isTableReady);

            string stockingLabel = modelFac.GetLabels(ShellPartType.Stocking).ElementAt(2);
            var stockingModel = modelFac.GetModels(ShellPartType.Stocking, stockingLabel).First();
            
            Assert.IsNotNull(stockingModel);

            string clothLabel = modelFac.GetLabels(ShellPartType.Cloth).ElementAt(8);
            var clothModel = modelFac.GetModels(ShellPartType.Cloth, clothLabel).First();

            Assert.IsNotNull(clothModel);

            ShellMaterialFactory matFac = new ShellMaterialFactory(LoggerMockFactory.CreateLogger<ShellMaterialFactory>(), _config, client);
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
                matStream = matFac.Overlap(orderedMaterials, new System.Drawing.Size(650, 1200));
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
 