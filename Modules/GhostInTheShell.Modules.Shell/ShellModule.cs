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
using GhostInTheShell.Modules.ShellInfra;
using GhostInTheShell.Modules.Shell.ViewModels;
using Prism.Services.Dialogs;
using Prism.Events;
using System.ComponentModel;
using GhostInTheShell.Modules.Script;
using Microsoft.Extensions.Logging;
using GhostInTheShell.Modules.Shell.Client;

namespace GhostInTheShell.Modules.Shell
{
    public sealed class ShellModule : IModule
    {
        readonly ILogger _logger;

        IShellService? _charClientSvc;
        MaterialCollectionChangedEvent? _matCollChangedEvent;

        public ShellModule(ILogger<ShellModule> logger)
        {
            _logger = logger;
        }

        public async void OnInitialized(IContainerProvider containerProvider)
        {
            _charClientSvc = containerProvider.Resolve<IShellService>();
            if(_charClientSvc is null)
            {
                _logger.Log(LogLevel.Error, "NullRef: ICharacterClientService");
                return;
            }

            var shellSize = await _charClientSvc.RequestShellSizeAsync(ShellNames.Kaori);
            if (shellSize == System.Drawing.Size.Empty)
            {
                _logger.Log(LogLevel.Error, "InvalidRes: ShellSize");
                return;
            }

            Task<byte[]?> reqImageTask = _charClientSvc.RequestShellImageAsync(ShellNames.Kaori, "부끄럼0", "중간-무광", "미소");
            var eventAggr = containerProvider.Resolve<IEventAggregator>();
            _matCollChangedEvent = eventAggr.GetEvent<MaterialCollectionChangedEvent>();

            var dialogSvc = containerProvider.Resolve<IDialogService>();
            dialogSvc.Show(nameof(ShellView), new DialogParameters { { nameof(ShellViewModel.ImageSize), shellSize } }, null, nameof(ShellWindow));

            var imgBytes = await reqImageTask.WaitAsync(TimeSpan.FromMilliseconds(10000));
            if (imgBytes is null)
            {
                _logger.Log(LogLevel.Error, $"NullRef: {nameof(imgBytes)}");
                return;
            }
            
            _matCollChangedEvent.Publish(new System.IO.MemoryStream(imgBytes));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IShellService, ShellService>();
            
            containerRegistry.RegisterDialog<ShellView, ShellViewModel>(nameof(ShellView));
            containerRegistry.RegisterDialogWindow<ShellWindow>(nameof(ShellWindow));
        }

        private async void onShellChangeExecute(ShellChangeScriptCommandEventArgs e)
        {
            var imgBytes = await _charClientSvc!.RequestShellImageAsync(ShellNames.Kaori, e.HeadLabel, e.EyeLabel, e.FaceLabel);

            if (imgBytes is null)
            {
                _logger.Log(LogLevel.Error, $"NullRef: {nameof(imgBytes)}"); ;
                return;
            }

            _matCollChangedEvent!.Publish(new System.IO.MemoryStream(imgBytes));
        }
    }
}
