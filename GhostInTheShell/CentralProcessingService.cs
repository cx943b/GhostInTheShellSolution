using GhostInTheShell.Modules.Balloon;
using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Script;
using GhostInTheShell.Modules.ShellInfra;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell
{
    public interface ICentralProcessingService
    {
        void ExecuteScript(string script);
    }

    internal class CentralProcessingService : ICentralProcessingService, IDisposable
    {
        readonly ILogger<CentralProcessingService> _logger;
        readonly IShellService _shellSvc;
        readonly IBalloonService _ballSvc;
        readonly IScriptService _scriptSvc;

        readonly List<SubscriptionToken> _lstSubToken = new List<SubscriptionToken>();
        readonly MaterialCollectionChangedEvent? _matCollChangeEvent = null;

        public bool IsPositionSync { get; set; }

        public CentralProcessingService(
            ILogger<CentralProcessingService> logger,
            IEventAggregator eventAggregator,
            IShellService shellSvc,
            IBalloonService ballSvc,
            IScriptService scriptSvc)
        {
            _logger = logger;
            _shellSvc = shellSvc ?? throw new NullReferenceException(nameof(shellSvc));
            _ballSvc = ballSvc ?? throw new NullReferenceException(nameof(ballSvc));
            _scriptSvc = scriptSvc ?? throw new NullReferenceException(nameof(scriptSvc));

            _matCollChangeEvent = eventAggregator.GetEvent<MaterialCollectionChangedEvent>();

            _lstSubToken.Add(eventAggregator.GetEvent<PrintWordScriptCommandEvent>().Subscribe(onPrintWordScriptCommandExecute,ThreadOption.UIThread));
            _lstSubToken.Add(eventAggregator.GetEvent<ClearWordsScriptCommandEvent>().Subscribe(() => _ballSvc.Clear(), ThreadOption.UIThread));

            _lstSubToken.Add(eventAggregator.GetEvent<ShellChangeScriptCommandEvent>().Subscribe(onShellChangeScriptCommandExecute));
            _lstSubToken.Add(eventAggregator.GetEvent<ShellPositionChangedEvent>().Subscribe(onShellPositionChanged));


        }

        public void Dispose()
        {
            _lstSubToken.ForEach(token => token.Dispose());
        }

        public void ExecuteScript(string script)
        {
            if (String.IsNullOrEmpty(script))
                return;

            _scriptSvc.Execute(script);
        }


        private void onPrintWordScriptCommandExecute(PrintWordScriptCommandEventArgs e)
        {
            _ballSvc.AddText(e.PrintWord);
        }
        private async void onShellChangeScriptCommandExecute(ShellChangeScriptCommandEventArgs e)
        {
            var imgBytes = await _shellSvc.RequestShellImageAsync(e.HeadLabel, e.EyeLabel, e.FaceLabel);
            if(imgBytes is not null)
            {
                _matCollChangeEvent!.Publish(new System.IO.MemoryStream(imgBytes));
            }
        }
        private void onShellPositionChanged(ShellPositionChangedEventArgs e)
        {
            if (!IsPositionSync)
                return;


            // balloonPositionChange
        }
    }

}
