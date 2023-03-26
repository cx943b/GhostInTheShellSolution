using GhostInTheShell.Modules.InfraStructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.ShellInfra
{
    // NotUse, Use gRPC
    public sealed class ShellInfraModule : IModule
    {
        const string FactoryTypeSection = "ShellData:FactoryType";
        const string ShellNameSection = "ShellData:Name";

        public async void OnInitialized(IContainerProvider containerProvider)
        {
            IConfiguration configuration= containerProvider.Resolve<IConfiguration>();
            string? factoryType = configuration.GetSection("FactoryTypeSection")?.Value;

            if (String.IsNullOrEmpty(factoryType))
                throw new NullReferenceException("NotFound: FactoryTypeSection");

            string? shellName = configuration.GetSection("ShellNameSection")?.Value;

            if (String.IsNullOrEmpty(shellName))
                throw new NullReferenceException("NotFound: ShellNameSection");


            var modelFac = containerProvider.Resolve<IShellModelFactory>();
            bool isMaterialReady = await modelFac.InitializeAsync(shellName);
            if (!isMaterialReady)
                throw new InvalidOperationException($"NotInitialized: {nameof(IShellMaterialFactory)}");

            var charSvc = containerProvider.Resolve<ICharacterService>();
            bool isCharacterReady =  await charSvc.InitializeAsync(shellName);
            if (!isCharacterReady)
                throw new InvalidOperationException($"NotInitialized: {nameof(ICharacterService)}");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IShellModelFactory>(prov =>
            {
                IConfiguration config = prov.Resolve<IConfiguration>();

                string? factoryType = config.GetSection("FactoryTypeSection")?.Value;
                if (String.IsNullOrEmpty(factoryType))
                    throw new NullReferenceException("NotFound: FactoryTypeSection");

                var logFac = prov.Resolve<ILoggerFactory>();

                if (String.Compare(factoryType, "Local", true) == 0)
                {
                    var logger = logFac.CreateLogger<ShellModelLocalFactory>();
                    return new ShellModelLocalFactory(logger, config);
                }
                else
                {
                    var client = prov.Resolve<HttpClient>();
                    var logger = logFac.CreateLogger<ShellModelRemoteFactory>();

                    return new ShellModelRemoteFactory(logger, config, client);
                }
            });
            containerRegistry.RegisterSingleton<IShellMaterialFactory>(prov =>
            {
                IConfiguration config = prov.Resolve<IConfiguration>();

                string? factoryType = config.GetSection("FactoryTypeSection")?.Value;
                if (String.IsNullOrEmpty(factoryType))
                    throw new NullReferenceException("NotFound: FactoryTypeSection");

                var logFac = prov.Resolve<ILoggerFactory>();

                if (String.Compare(factoryType, "Local", true) == 0)
                {
                    var logger = logFac.CreateLogger<ShellMaterialLocalFactory>();
                    return new ShellMaterialLocalFactory(logger, config);
                }
                else
                {
                    var client = prov.Resolve<HttpClient>();
                    var logger = logFac.CreateLogger<ShellMaterialRemoteFactory>();

                    return new ShellMaterialRemoteFactory(logger, config, client);
                }
            });
            containerRegistry.RegisterSingleton<ICharacterService, CharacterRemoteService>();
            //containerRegistry.RegisterSingleton<ICharacterService>(prov =>
            //{
            //    IConfiguration config = prov.Resolve<IConfiguration>();

            //    string? factoryType = config.GetSection("FactoryTypeSection")?.Value;
            //    if (String.IsNullOrEmpty(factoryType))
            //        throw new NullReferenceException("NotFound: FactoryTypeSection");

            //    var logFac = prov.Resolve<ILoggerFactory>();
            //    var matFac = prov.Resolve<IShellMaterialFactory>();
            //    var modelFac = prov.Resolve<IShellModelFactory>();
            //    var eventAggr = prov.Resolve<IEventAggregator>();

            //    if (String.Compare(factoryType, "Local", true) == 0)
            //    {
            //        var logger = logFac.CreateLogger<CharacterLocalService>();
            //        return new CharacterLocalService(logger, eventAggr, config, modelFac, matFac);
            //    }
            //    else
            //    {
            //        var client = prov.Resolve<HttpClient>();
            //        var logger = logFac.CreateLogger<CharacterRemoteService>();

            //        return new CharacterRemoteService(logger, eventAggr, config, modelFac, matFac, client);
            //    }
            //});
        }
    }
}
