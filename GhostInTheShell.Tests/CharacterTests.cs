using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            bool isAppliedColor = charSvc.ChangePartColor(ShellPartType.Cloth, new Modules.InfraStructure.Hsl(0, 0.7, 1));
            Assert.IsTrue(isAppliedColor);

            isAppliedColor = charSvc.ChangePartColor(ShellPartType.Eye, new Modules.InfraStructure.Hsl(0, 1, 1));
            Assert.IsTrue(isAppliedColor);

            isAppliedColor = charSvc.ChangePartColor(ShellPartType.Face, new Modules.InfraStructure.Hsl(0, 0, 0.5));
            Assert.IsTrue(isAppliedColor);

            isAppliedColor = charSvc.ChangePartColor(ShellPartType.FrontHair, new Modules.InfraStructure.Hsl(0, 0, 0.5));
            Assert.IsTrue(isAppliedColor);
            isAppliedColor = charSvc.ChangePartColor(ShellPartType.BackHair, new Modules.InfraStructure.Hsl(0, 0, 0.5));
            Assert.IsTrue(isAppliedColor);
            isAppliedColor = charSvc.ChangeAccessoryColor(ShellPartType.AttachHair, new Modules.InfraStructure.Hsl(0, 0, 0.5));
            Assert.IsTrue(isAppliedColor);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Load excpt on Face
            string clothLabel = "반바지민소매사복";
            bool isClothChanged = await charSvc.ChangeCloth(clothLabel);
            Assert.IsTrue(isClothChanged);

            string underwearLabel = "04(下着)ﾌﾞﾗ0K水";
            bool isUnderwearChanged = await charSvc.ChangeUnderwear(underwearLabel);
            Assert.IsTrue(isUnderwearChanged);

            bool isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.Socks, "[靴下] 長0白");
            Assert.IsTrue(isAccessoryAdded);
            isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.ShoesEx, "[靴] ｽﾆｰｶｰ0赤前");
            Assert.IsTrue(isAccessoryAdded);
            isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.ShoesEx, "[靴] ｽﾆｰｶｰ0赤後");
            Assert.IsTrue(isAccessoryAdded);
            isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.AttachHair, "[髪b] 40L(髪)");
            Assert.IsTrue(isAccessoryAdded);
            isAccessoryAdded = await charSvc.AddAccessory(ShellPartType.AttachHair, "[髪b] 40R(髪)");
            Assert.IsTrue(isAccessoryAdded);

            string frontHairLabel = "ヒタム・キャン";
            bool isFrontHairChanged = await charSvc.ChangeFrontHair(frontHairLabel);
            Assert.IsTrue(isFrontHairChanged);

            string backHairLabel = "ヒタム・キャン";
            bool isBackHairChanged = await charSvc.ChangeBackHair(backHairLabel);
            Assert.IsTrue(isBackHairChanged);

            watch.Stop();
            Debug.WriteLine($"Load excpt on FacePart: {watch.ElapsedMilliseconds}ms");
            // Load excpt on Face

            // Load Face
            watch.Restart();
            string headLabel = "부끄럼0";
            bool isHeadChanged = await charSvc.ChangeHead(headLabel);
            Assert.IsTrue(isHeadChanged);

            string faceLabel = "미소";
            bool isFaceChanged = await charSvc.ChangeFace(faceLabel);
            Assert.IsTrue(isFaceChanged);

            string eyeLabel = "열림";
            bool isEyeChanged = await charSvc.ChangeEye(eyeLabel);
            Assert.IsTrue(isEyeChanged);

            watch.Stop();
            Debug.WriteLine($"Load FacePart: {watch.ElapsedMilliseconds}ms");
            // Load Face

            var orderedMaterials = charSvc.GetMaterials()
                .OrderBy(m => m.MainIndex)
                .ThenBy(m => m.SubIndex);

            MemoryStream? matStream = null;
            FileStream? fs = null;

            try
            {
                watch.Restart();
                matStream = matFac.Overlap(orderedMaterials, charSvc.ShellSize);
                Assert.IsNotNull(matStream);

                watch.Stop();
                Debug.WriteLine($"Overlaped: {watch.ElapsedMilliseconds}ms");

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
