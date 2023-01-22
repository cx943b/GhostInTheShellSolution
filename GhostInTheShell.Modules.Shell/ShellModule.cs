using GhostInTheShell.Modules.InfraStructure;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Shell
{
    public sealed class ShellModule : IModule
    {
        const string ShellName = "Kaori";

        public async void OnInitialized(IContainerProvider containerProvider)
        {
            var modelFac = containerProvider.Resolve<IShellModelFactory>();
            await modelFac.InitializeAsync(ShellName);

            var regionMgr = containerProvider.Resolve<IRegionManager>();
            regionMgr.RegisterViewWithRegion(WellknownRegionNames.ShellViewRegion, nameof(Views.ShellView));

            var charSvc = containerProvider.Resolve<ICharacterServiceV2>();
            await charSvc.InitializeAsync("Kaori");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        { 
            containerRegistry.RegisterSingleton<IShellModelFactory, ShellModelFactory>();
            containerRegistry.RegisterSingleton<IShellMaterialFactory, ShellMaterialFactory>();
            containerRegistry.RegisterSingleton<ICharacterServiceV2, ICharacterServiceV2>();

            containerRegistry.RegisterDialogWindow<ShellWindow>();
        }
    }
}
