using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.Script;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prism;
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

        protected override void InitializeModules()
        {
            base.InitializeModules();

            var regionMgr = Container.Resolve<RegionManager>();
            regionMgr.RegisterViewWithRegion<Views.MainView>(WellknownRegionNames.MainViewRegion);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule<ScriptModule>();
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

            //containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
