using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Modules.Shell.Models;
using GhostInTheShell.Modules.Shell.ViewModels;
using Microsoft.Extensions.Configuration;
using Moq;
using Prism.Events;
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
    // https://arctouch.com/blog/moq-unit-testing-prism-eventaggregator
    [TestClass]
    public class CharacterTests
    {
        const string ShellName = "Kaori";//"Fumino";

        readonly Stopwatch _watch = new Stopwatch();
        IConfiguration _config;
        Mock<IEventAggregator> _moEventAggr;
        Mock<MaterialCollectionChangedEvent> _moMatCollChangedEvent;

        [TestInitialize]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                   .AddJsonFile("G:\\SosoProjects\\GhostInTheShellSolution\\GhostInTheShell\\AppSettings.json")
                   .Build();

            _moMatCollChangedEvent = new Mock<MaterialCollectionChangedEvent>();
            _moMatCollChangedEvent
                .Setup(e => e.Publish(It.IsAny<MemoryStream>()))
                .Callback<MemoryStream>(ms => onMaterialCollectionChanged(ms));

            _moEventAggr = new Mock<IEventAggregator>();
            _moEventAggr
                .Setup(ea => ea.GetEvent<MaterialCollectionChangedEvent>())
                .Returns(_moMatCollChangedEvent.Object);
        }

        [TestMethod]
        public async Task CharacterInitTestV2()
        {
            HttpClient client = new HttpClient();

            IEventAggregator eventAggregator = _moEventAggr.Object;

            ShellModelFactory modelFac = new ShellModelFactory(LoggerMockFactory.CreateLogger<ShellModelFactory>(), _config, client);
            ShellMaterialFactory matFac = new ShellMaterialFactory(LoggerMockFactory.CreateLogger<ShellMaterialFactory>(), _config, client);

            CharacterServiceV2 charSvc = new CharacterServiceV2(LoggerMockFactory.CreateLogger<CharacterServiceV2>(), eventAggregator, _config, modelFac, matFac, client);
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

            IDictionary<ShellPartType, string> dicChangePart = new Dictionary<ShellPartType, string>()
            {
                { ShellPartType.Cloth, "반바지민소매사복" },
                { ShellPartType.Underwear, "04(下着)ﾌﾞﾗ0K水" },
                { ShellPartType.FrontHair, "ヒタム・キャン" },
                { ShellPartType.BackHair, "ヒタム・キャン" },
                { ShellPartType.Head, "부끄럼0" },
                { ShellPartType.Face, "미소" },
                { ShellPartType.Eye, "열림" }
            };

            IEnumerable<AccessoryAddPair> accessoryAddPairs = new AccessoryAddPair[]
            {
                new AccessoryAddPair(ShellPartType.Socks, "[靴下] 長0白"),
                new AccessoryAddPair(ShellPartType.ShoesEx, "[靴] ｽﾆｰｶｰ0赤前"),
                new AccessoryAddPair(ShellPartType.ShoesEx, "[靴] ｽﾆｰｶｰ0赤後"),
                new AccessoryAddPair(ShellPartType.AttachHair, "[髪b] 40L(髪)"),
                new AccessoryAddPair(ShellPartType.AttachHair, "[髪b] 40R(髪)")
            };

            _watch.Start();

            await charSvc.ChangeShell(dicChangePart, accessoryAddPairs);
        }

        private void onMaterialCollectionChanged(MemoryStream ms)
        {
            _watch.Stop();
            Debug.WriteLine($"ImageOverlaped: {_watch.ElapsedMilliseconds}ms");

            using FileStream fs = new FileStream("Test.png", FileMode.Create, FileAccess.Write);
            ms.Position = 0;
            ms.CopyTo(fs);
        }
    }
}
