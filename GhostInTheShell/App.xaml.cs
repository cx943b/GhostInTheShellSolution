using System;
using System.Windows;
using GhostInTheShell.Modules.Balloon;
using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.MvvmInfra;
using GhostInTheShell.Modules.Script;
using GhostInTheShell.Modules.Shell;
using GhostInTheShell.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
            shell.Width = 500;
            shell.Height = 400;

            shell.Show();
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();

            var regionMgr = Container.Resolve<IRegionManager>();
            regionMgr.RegisterViewWithRegion<MainView>(WellknownRegionNames.MainViewRegion);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule<ScriptModule>();
            moduleCatalog.AddModule<MvvmInfraModule>();
            moduleCatalog.AddModule<ShellModule>();
            moduleCatalog.AddModule<BalloonModule>();
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
            containerRegistry.RegisterSingleton<ICentralProcessingService, CentralProcessingService>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);

            var adapter = Container.Resolve<BalloonItemsControlAdapter>();
            regionAdapterMappings.RegisterMapping<BalloonItemsControl>(adapter);
        }
    }
}
