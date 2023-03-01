using GhostInTheShell.Modules.Balloon.Views;
using GhostInTheShell.Modules.InfraStructure;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Balloon
{
    public class BalloonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        //    var regionMgr = containerProvider.Resolve<IRegionManager>();
        //    regionMgr.RegisterViewWithRegion(WellknownRegionNames.BalloonViewRegion, typeof(BalloonView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<IBalloonService, BalloonService>();
        }
    }
}
