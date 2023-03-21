using GhostInTheShell.Modules.Balloon.ViewModels;
using GhostInTheShell.Modules.Balloon.Views;
using GhostInTheShell.Modules.InfraStructure;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
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
            IRegionManager regionMgr = containerProvider.Resolve<IRegionManager>();
            var result = regionMgr.RegisterViewWithRegion<BalloonContentView>(WellknownRegionNames.BalloonContentViewRegion);

            var dialogSvc = containerProvider.Resolve<IDialogService>();
            dialogSvc.Show(nameof(BalloonView),null, null, nameof(BalloonWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IBalloonService, BalloonService>();

            containerRegistry.RegisterDialog<BalloonView, BalloonViewModel>(nameof(BalloonView));
            containerRegistry.RegisterDialogWindow<BalloonWindow>(nameof(BalloonWindow));
        }
    }
}
