using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.Script;
using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Modules.ShellInfra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

using Serilog;

namespace GhostInTheShell
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);

            shell.Show();
        }

        protected override async void InitializeModules()
        {
            base.InitializeModules();

            var regionMgr = Container.Resolve<RegionManager>();
            regionMgr.RegisterViewWithRegion<Views.MainView>(WellknownRegionNames.MainViewRegion);

            var shellWindow = Container.Resolve<ShellWindow>();
            shellWindow.Show();


            // Error
            //var mainWindow = Container.Resolve<MainWindow>();
            //shellWindow.Owner = mainWindow;


            var charClientSvc = Container.Resolve<ICharacterClientService>();


            // InitSize
            var shellSize = await charClientSvc.RequestCharacterSize();

            if (shellSize == System.Drawing.Size.Empty)
                throw new InvalidOperationException("InvalidRes: ShellSize");

            shellWindow.Width = shellSize.Width;
            shellWindow.Height = shellSize.Height;
            shellWindow.Left = SystemParameters.WorkArea.Width - shellSize.Width;
            shellWindow.Top = SystemParameters.WorkArea.Height - shellSize.Height;
            // InitSize

            // InitImage
            var eventAggr = Container.Resolve<IEventAggregator>();
            eventAggr.GetEvent<ShellSizeChangedEvent>().Publish(shellSize);

            var imgBytes = await charClientSvc.RequestCharacterImage("부끄럼0", "중간-무광", "미소");
            
            if(imgBytes is null)
                throw new NullReferenceException(nameof(imgBytes));

            eventAggr.GetEvent<MaterialCollectionChangedEvent>().Publish(new System.IO.MemoryStream(imgBytes));
            // InitImage



            eventAggr.GetEvent<ShellChangeScriptCommandEvent>().Subscribe(onShellChangeExecute, ThreadOption.UIThread);
        }

        private async void onShellChangeExecute(ShellChangeScriptCommandEventArgs e)
        {
            var charClientSvc = Container.Resolve<ICharacterClientService>();

            var imgBytes = await charClientSvc.RequestCharacterImage(e.HeadLabel, e.EyeLabel, e.FaceLabel);

            if (imgBytes is null)
                throw new NullReferenceException(nameof(imgBytes));

            var eventAggr = Container.Resolve<IEventAggregator>();
            eventAggr.GetEvent<MaterialCollectionChangedEvent>().Publish(new System.IO.MemoryStream(imgBytes));
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule<ScriptModule>();
            moduleCatalog.AddModule<ShellModule>();
        }

        

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IConfiguration>(() =>
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            });
            containerRegistry.RegisterSingleton<ILoggerFactory>(prov =>
            {
                var config = prov.Resolve<IConfiguration>() ?? throw new NullReferenceException("Couldn't resolve IConfiguration");

                return LoggerFactory.Create(builder =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(config)
                        .CreateLogger();

                    builder
                        .ClearProviders()
                        .AddSerilog(logger)
                        .AddDebug();
                });
            });

            containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
