using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.Shell.Views;

namespace GhostInTheShell.Modules.Shell
{
    public sealed class ShellModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionMgr = containerProvider.Resolve<IRegionManager>();
            regionMgr.RegisterViewWithRegion<ShellView>(WellknownRegionNames.ShellViewRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialogWindow<ShellWindow>();
        }
    }
}
