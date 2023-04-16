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
        readonly ILogger _logger;

        public BalloonModule(ILogger<BalloonModule> logger)
        {
            _logger = logger;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var charNameSvc = containerProvider.Resolve<ICharacterNameService>();
            if(charNameSvc is null)
            {
                _logger.Log(LogLevel.Error, $"NullRef: {nameof(charNameSvc)}");
                return;
            }

            IEnumerable<string> charNames = charNameSvc.CharacterNames;
            if(charNames.Any())
            {
                var dialogSvc = containerProvider.Resolve<IDialogService>();

                foreach(var charName in charNames)
                    prepareBalloon(charName, dialogSvc);
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"EmptyArray: {nameof(charNames)}");
            }
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IBalloonService, BalloonService>();

            containerRegistry.RegisterDialog<BalloonView, BalloonViewModel>(CharacterNames.Fumino + nameof(BalloonView));
            containerRegistry.RegisterDialog<BalloonView, BalloonViewModel>(CharacterNames.Kaori + nameof(BalloonView));
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
