using GhostInTheShell.Modules.Balloon;
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
        readonly DelegateCommand _AddTextCommand;
        readonly DelegateCommand _AddImageCommand;
        private readonly IBalloonService _ballSvc;

        public ICommand AddTextCommand => _AddTextCommand;
        public ICommand AddImageCommand => _AddImageCommand;

        public MainViewModel(IBalloonService ballSvc)
        {
            _AddTextCommand = new DelegateCommand(onAddTextExecute);
            _AddImageCommand = new DelegateCommand(onAddImageExecute);

            _ballSvc = ballSvc;
        }

        private void onAddTextExecute()
        {
            _ballSvc.AddText("끄아앙!");
        }
        private void onAddImageExecute()
        {
            _ballSvc.AddImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/8/85/Smiley.svg/1200px-Smiley.svg.png"));
        }
    }


    //internal class MainViewModel : BindableBase
    //{
    //    readonly IEventAggregator _eventAggregator;
    //    readonly IScriptService _scriptService;

    //    private string? _Text;
    //    public string? Text
    //    {
    //        get { return _Text; }
    //        set { SetProperty(ref _Text, value); }
    //    }


    //    // \t[0]\s[보통, 열림, 웃음]벼룩의 간도 \w[500]\s[보통, 열림, 웃음]모이면 짭짤하구만
    //    // \t[0]\s[보통, 중간-암흑, 불만]카에데를 방패로 개같은 개그를 치는 건 그만둬
    //    // \t[0]\s[보통, ㅇ, 싫음]바람이 불면 나뭇잎이 흔들리는데 \w[300]\s[보통, 열림-무광, 의심]어떻게 사과가 빨갛다고 하냐?

    //    private string? _InputScript = "\\t[0]\\s[부끄럼0, 열림, 웃음]5초만에, \\w[1000]\\s[보통, 중간, 미소]6초를 세어주마.";
    //    public string? InputScript
    //    {
    //        get { return _InputScript; }
    //        set { SetProperty(ref _InputScript, value); }
    //    }

    //    private DelegateCommand _ExecuteCommand;




    //    public ICommand ExecuteCommand => _ExecuteCommand;

    //    public MainViewModel(ILogger<MainViewModel> logger, IEventAggregator eventAggregator, IScriptService scriptService)
    //    {
    //        _eventAggregator = eventAggregator;
    //        _scriptService = scriptService;

    //        _ExecuteCommand = new DelegateCommand(OnExecute, () => !String.IsNullOrEmpty(InputScript)).ObservesProperty(() => this.InputScript);

    //        eventAggregator.GetEvent<PrintWordScriptCommandEvent>().Subscribe(e => Text += e.PrintWord);
            
    //        //logger.LogTrace("MainViewModel Initialized");
    //    }

        

    //    private void OnExecute()
    //    {
    //        Text = "";

    //        _scriptService.Execute(_InputScript!);
    //    }
    //}
}
