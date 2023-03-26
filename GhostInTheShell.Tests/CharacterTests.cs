using GhostInTheShell.Modules.ShellInfra;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using Prism.Events;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Media.Imaging;

namespace GhostInTheShell.Tests
{
    public class WpfTestMethod : TestMethodAttribute
    {
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            TestResult[]? results = null;
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                results = new TestResult[] { testMethod.Invoke(testMethod.Arguments) };

            Thread t = new Thread(() =>
            {
                results = new TestResult[] { testMethod.Invoke(testMethod.Arguments) };
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            t.Join();

            if (results == null)
                return new TestResult[0];

            return results;
        }
    }

    // https://arctouch.com/blog/moq-unit-testing-prism-eventaggregator
    [TestClass]
    public class CharacterTests
    {
        const string ShellName = "Kaori";//"Fumino";
        //const string ShellName = "Fumino";

        readonly Stopwatch _watch = new Stopwatch();
        IConfiguration _config;

        //ShellViewModel _kaoriViewModel;
        //ShellViewModel _fuminoViewModel;

        Mock<IEventAggregator> _moKaoriEventAggr;
        Mock<IEventAggregator> _moFuminoEventAggr;
        Mock<MaterialCollectionChangedEvent> _moKaoriMatCollChangedEvent;
        Mock<MaterialCollectionChangedEvent> _moFuminoMatCollChangedEvent;


        Mock<ShellSizeChangedEvent> _moShellSizeChangedEvent;

        [TestInitialize]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                   .AddJsonFile("G:\\SosoProjects\\GhostInTheShellSolution\\GhostInTheShell\\AppSettings.json")
                   .Build();

            //_moKaoriMatCollChangedEvent = new Mock<MaterialCollectionChangedEvent>();
            //_moKaoriMatCollChangedEvent
            //    .Setup(e => e.Publish(It.IsAny<MemoryStream>()))
            //    .Callback<MemoryStream>(ms => onKaoriMaterialCollectionChanged(ms));

            //_moFuminoMatCollChangedEvent = new Mock<MaterialCollectionChangedEvent>();
            //_moFuminoMatCollChangedEvent
            //    .Setup(e => e.Publish(It.IsAny<MemoryStream>()))
            //    .Callback<MemoryStream>(ms => onFuminoMaterialCollectionChanged(ms));

            //_moShellSizeChangedEvent = new Mock<ShellSizeChangedEvent>();
            //_moShellSizeChangedEvent
            //    .Setup(e => e.Publish(It.IsAny<System.Drawing.Size>()))
            //    .Callback<System.Drawing.Size>(size => Debug.WriteLine($"ShellSize: {size}"));

            //_moKaoriEventAggr = new Mock<IEventAggregator>();
            //_moKaoriEventAggr
            //    .Setup(ea => ea.GetEvent<MaterialCollectionChangedEvent>())
            //    .Returns(_moKaoriMatCollChangedEvent.Object);
            //_moKaoriEventAggr
            //    .Setup(ea => ea.GetEvent<ShellSizeChangedEvent>())
            //    .Returns(_moShellSizeChangedEvent.Object);

            //_moFuminoEventAggr = new Mock<IEventAggregator>();
            //_moFuminoEventAggr
            //    .Setup(ea => ea.GetEvent<MaterialCollectionChangedEvent>())
            //    .Returns(_moFuminoMatCollChangedEvent.Object);
            //_moFuminoEventAggr
            //    .Setup(ea => ea.GetEvent<ShellSizeChangedEvent>())
            //    .Returns(_moShellSizeChangedEvent.Object);
        }


        [TestMethod]
        public async Task CharacterClientTest()
        {
            ShellService clientSvc = new CharacterClientService(LoggerMockFactory.CreateLogger<ShellService>());
            byte[]? charBytes = await clientSvc.RequestShellImageAsync("부끄럼0", "중간", "웃음");

            Assert.IsNotNull(charBytes);

            FileStream fs = new FileStream("CharacterClientTest.png", FileMode.Create, FileAccess.Write);
            await fs.WriteAsync(charBytes, 0, charBytes.Length);
        }



        [TestMethod]
        public async Task CharacterInitTest()
        {
            HttpClient client = new HttpClient();

            IEventAggregator eventAggregator = _moKaoriEventAggr.Object;

            ShellModelRemoteFactory modelFac = new ShellModelRemoteFactory(LoggerMockFactory.CreateLogger<ShellModelRemoteFactory>(), _config, client);
            bool isModelFactoryReady = await modelFac.InitializeAsync(ShellName);
            Assert.IsTrue(isModelFactoryReady);

            ShellMaterialRemoteFactory matFac = new ShellMaterialRemoteFactory(
                LoggerMockFactory.CreateLogger<ShellMaterialRemoteFactory>(), _config, client);
            ICharacterService charSvc = new CharacterRemoteService(
                LoggerMockFactory.CreateLogger<CharacterRemoteService>(), eventAggregator, _config, modelFac, matFac, client);

            _watch.Start();

            bool isCharServiceReady = await charSvc.InitializeAsync(ShellName);
            Assert.IsTrue(isCharServiceReady);

            // For wait onMaterialCollectionChanged
            Thread.Sleep(1000);
        }

        /*
        [WpfTestMethod]
        public void ShowCharacterToWindow()
        {
            _kaoriViewModel = new ShellViewModel(_moKaoriEventAggr.Object);

            ShellView kaoriView = new ShellView();
            kaoriView.DataContext = _kaoriViewModel;

            ShellWindow w = new ShellWindow();
            w.Loaded += onKaoriShellWindowLoaded;
            w.Closed += (s, e) => System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            w.Content = kaoriView;

            w.Show();


            _fuminoViewModel = new ShellViewModel(_moFuminoEventAggr.Object);


            ShellView fuminoView = new ShellView();
            fuminoView.DataContext = _fuminoViewModel;

            ShellWindow fuminoWindow = new ShellWindow();
            fuminoWindow.Loaded += onFuminoShellWindowLoaded;
            fuminoWindow.Closed += (s, e) => System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            fuminoWindow.Content = fuminoView;

            fuminoWindow.Show();



            System.Windows.Threading.Dispatcher.Run();
        }

        private async void onKaoriShellWindowLoaded(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            IEventAggregator eventAggregator = _moKaoriEventAggr.Object;

            ShellModelRemoteFactory modelFac = new ShellModelRemoteFactory(LoggerMockFactory.CreateLogger<ShellModelRemoteFactory>(), _config, client);
            bool isModelFactoryReady = await modelFac.InitializeAsync("Kaori");
            Assert.IsTrue(isModelFactoryReady);

            ShellMaterialRemoteFactory matFac = new ShellMaterialRemoteFactory(
                LoggerMockFactory.CreateLogger<ShellMaterialRemoteFactory>(), _config, client);
            CharacterRemoteService charSvc = new CharacterRemoteService(
                LoggerMockFactory.CreateLogger<CharacterRemoteService>(), eventAggregator, _config, modelFac, matFac, client);

            _watch.Start();
            bool isCharServiceReady = await charSvc.InitializeAsync("Kaori");
            Assert.IsTrue(isCharServiceReady);

            var shellWidth = charSvc.ShellSize.Width;
            var shellHeight = charSvc.ShellSize.Height;
            _kaoriViewModel.ShellSize = charSvc.ShellSize;

            Window w = ((Window)sender);
            w.Width = shellWidth;
            w.Height = shellHeight;
            w.Left = SystemParameters.WorkArea.Width - shellWidth + 100;
            w.Top = SystemParameters.WorkArea.Height - shellHeight + 20;
        }
        private async void onFuminoShellWindowLoaded(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            IEventAggregator eventAggregator = _moFuminoEventAggr.Object;

            ShellModelRemoteFactory modelFac = new ShellModelRemoteFactory(LoggerMockFactory.CreateLogger<ShellModelRemoteFactory>(), _config, client);
            bool isModelFactoryReady = await modelFac.InitializeAsync("Fumino");
            Assert.IsTrue(isModelFactoryReady);

            ShellMaterialRemoteFactory matFac = new ShellMaterialRemoteFactory(
                LoggerMockFactory.CreateLogger<ShellMaterialRemoteFactory>(), _config, client);
            CharacterRemoteService charSvc = new CharacterRemoteService(
                LoggerMockFactory.CreateLogger<CharacterRemoteService>(), eventAggregator, _config, modelFac, matFac, client);

            _watch.Start();
            bool isCharServiceReady = await charSvc.InitializeAsync("Fumino");
            Assert.IsTrue(isCharServiceReady);

            var shellWidth = charSvc.ShellSize.Width;
            var shellHeight = charSvc.ShellSize.Height;
            _fuminoViewModel.ShellSize = charSvc.ShellSize;

            Window w = ((Window)sender);
            w.Width = shellWidth;
            w.Height = shellHeight;
            w.Left = SystemParameters.WorkArea.Width - shellWidth + 100 - 300;
            w.Top = SystemParameters.WorkArea.Height - shellHeight;
        }

        private void onKaoriMaterialCollectionChanged(MemoryStream ms)
        {
            _watch.Stop();
            Debug.WriteLine($"ImageOverlaped: {_watch.ElapsedMilliseconds}ms");

            using FileStream fs = new FileStream("Test.png", FileMode.Create, FileAccess.Write);
            ms.Position = 0;
            ms.CopyTo(fs);

            if (_kaoriViewModel is not null)
            {
                ms.Position = 0;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                _kaoriViewModel.ShellSource = bi;
            }
        }
        private void onFuminoMaterialCollectionChanged(MemoryStream ms)
        {
            _watch.Stop();
            Debug.WriteLine($"ImageOverlaped: {_watch.ElapsedMilliseconds}ms");

            //using FileStream fs = new FileStream("Test.png", FileMode.Create, FileAccess.Write);
            //ms.Position = 0;
            //ms.CopyTo(fs);

            if (_fuminoViewModel is not null)
            {
                ms.Position = 0;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                _fuminoViewModel.ShellSource = bi;
            }
        }*/
    }
}