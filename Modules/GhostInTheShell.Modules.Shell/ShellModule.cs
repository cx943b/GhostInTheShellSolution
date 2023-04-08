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
using Microsoft.Extensions.Configuration;
using GhostInTheShell.Modules.MvvmInfra;

namespace GhostInTheShell.Modules.Shell
{
    public sealed class ShellModule : IModule
    {
        const string ShellNamesSectionName = "Shell:Names";
        readonly ILogger _logger;

        public ShellModule(ILogger<ShellModule> logger)
        {
            _logger = logger;
        }

        public async void OnInitialized(IContainerProvider containerProvider)
        {
            var config = containerProvider.Resolve<IConfiguration>();
            var sShellNames = config[ShellNamesSectionName];

            if(String.IsNullOrEmpty(sShellNames))
            {
                _logger.Log(LogLevel.Error, $"NotFound: {nameof(ShellNamesSectionName)}");
                return;
            }

            string[] shellNames = sShellNames.Split('&', StringSplitOptions.RemoveEmptyEntries);
            if (sShellNames.Length == 0)
            {
                _logger.Log(LogLevel.Error, $"EmptyArray: {nameof(shellNames)}");
                return;
            }

            var shellSvc = containerProvider.Resolve<IShellService>();
            if(shellSvc is null)
            {
                _logger.Log(LogLevel.Error, $"NullRef: {nameof(shellSvc)}");
                return;
            }

            var eventAggr = containerProvider.Resolve<IEventAggregator>();
            var dialogSvc = containerProvider.Resolve<IDialogService>();
            var matCollChangedEvent = eventAggr.GetEvent<MaterialCollectionChangedEvent>();

            foreach (string shellName in shellNames)
                await prepareShell(shellName, shellSvc, matCollChangedEvent, eventAggr, dialogSvc);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IShellService, ShellService>();
            
            containerRegistry.RegisterDialog<ShellView, ShellViewModel>(ShellNames.Kaori + nameof(ShellView));
            containerRegistry.RegisterDialog<ShellView, ShellViewModel>(ShellNames.Fumino + nameof(ShellView));
            containerRegistry.RegisterDialogWindow<ShellWindow>(nameof(ShellWindow));
        }

        private async Task prepareShell(string shellName, IShellService shellSvc, MaterialCollectionChangedEvent matCollChangedEvent, IEventAggregator eventAggr, IDialogService dialogSvc)
        {
            var shellSize = await shellSvc.RequestShellSizeAsync(shellName);
            if (shellSize == System.Drawing.Size.Empty)
            {
                _logger.Log(LogLevel.Error, "InvalidRes: ShellSize");
                return;
            }

            Task<byte[]?> reqImageTask = shellSvc.RequestShellImageAsync(shellName, "부끄럼0", "중간-무광", "미소");

            DialogParameters dialParams = new DialogParameters
            {
                { nameof(ShellViewModel.ImageSize), shellSize },
                { nameof(ShellViewModel.Identifier), shellName }
            };

            dialogSvc.Show(shellName + nameof(ShellView), dialParams, null, nameof(ShellWindow));

            var imgBytes = await reqImageTask.WaitAsync(TimeSpan.FromMilliseconds(10000));
            if (imgBytes is null)
            {
                _logger.Log(LogLevel.Error, $"NullRef: {nameof(imgBytes)}");
                return;
            }

            matCollChangedEvent.Publish(new MaterialCollectionChangedEventArgs(shellName, new System.IO.MemoryStream(imgBytes)));
        }
    }
}
