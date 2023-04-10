using GhostInTheShell.Modules.Balloon.ViewModels;
using GhostInTheShell.Modules.Balloon.Views;
using GhostInTheShell.Modules.InfraStructure;
using Microsoft.Extensions.Logging;
using Prism.Events;
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
            //regionMgr.RegisterViewWithRegion<BalloonContentView>(WellknownRegionNames.BalloonContentViewRegion);

            var dialogSvc = containerProvider.Resolve<IDialogService>();
            prepareBalloon(ShellNames.Fumino, dialogSvc);
            prepareBalloon(ShellNames.Kaori, dialogSvc);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IBalloonService, BalloonService>();

            containerRegistry.RegisterDialog<BalloonView, BalloonViewModel>(ShellNames.Fumino + nameof(BalloonView));
            containerRegistry.RegisterDialog<BalloonView, BalloonViewModel>(ShellNames.Kaori + nameof(BalloonView));
            containerRegistry.RegisterDialogWindow<BalloonWindow>(nameof(BalloonWindow));
        }

        private void prepareBalloon(string balloonName, IDialogService dialogSvc)
        {
            DialogParameters dialParams = new DialogParameters
            {
                { nameof(BalloonViewModel.Identifier), balloonName }
            };

            dialogSvc.Show(balloonName + nameof(BalloonView), dialParams, null, nameof(BalloonWindow));
        }
    }
}
