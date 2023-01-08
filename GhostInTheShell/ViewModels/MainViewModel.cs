using GhostInTheShell.Modules.Script;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GhostInTheShell.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        readonly IEventAggregator _eventAggregator;
        readonly IScriptService _scriptService;

        private string? _Text;
        public string? Text
        {
            get { return _Text; }
            set { SetProperty(ref _Text, value); }
        }

        private DelegateCommand _ExecuteCommand;




        public ICommand ExecuteCommand => _ExecuteCommand;

        public MainViewModel(ILogger<MainViewModel> logger, IEventAggregator eventAggregator, IScriptService scriptService)
        {
            _eventAggregator = eventAggregator;
            _scriptService = scriptService;

            _ExecuteCommand = new DelegateCommand(OnExecute);

            eventAggregator.GetEvent<PrintWordScriptCommandEvent>().Subscribe(e => Text += e.PrintWord);

            logger.LogTrace("MainViewModel Initialized");
        }

        private void OnExecute()
        {
            Text = "";

            const string script = "\\t[0]이쁜 여자\\p[은;는] 흔하지만\\w[1000] \\t[1]이쁜 남자\\p[은;는] 귀하다구";
            _scriptService.Execute(script);
        }
    }
}
