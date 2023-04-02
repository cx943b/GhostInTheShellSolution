using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Balloon.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GhostInTheShell.Modules.Balloon.ViewModels
{
    public sealed class FuminoBalloonViewModel : BalloonViewModel
    {
        public const string CharacterName = "Fumino";

        public FuminoBalloonViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }

    }




    public class BalloonViewModel : BindableBase, IDialogAware, IDisposable
    {
        BalloonTailDirection _TailDirection = BalloonTailDirection.Right;

        double _TailPosition = 50;
        double _Width = 400;

        readonly List<SubscriptionToken> _lstEventToken = new List<SubscriptionToken>();

        public event Action<IDialogResult> RequestClose = null!;

        public BalloonTailDirection TailDirection
        {
            get => _TailDirection;
            set => SetProperty(ref _TailDirection, value);
        }
        public double TailPosition
        {
            get => _TailPosition;
            set => SetProperty(ref _TailPosition, value);
        }
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        

        public string Title => "BalloonDialog";

        public BalloonViewModel(IEventAggregator eventAggregator)
        {
            _lstEventToken.Add(eventAggregator.GetEvent<BalloonTailDirectionChangeEvent>().Subscribe(onBalloonTailDirectionChanged));
        }

        

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            _lstEventToken.ForEach(token => token.Dispose());
            _lstEventToken.Clear();
        }

        private void onBalloonTailDirectionChanged(BalloonTailDirection? tailDirection)
        {
            if (!tailDirection.HasValue)
                throw new ArgumentNullException(nameof(TailDirection));

            TailDirection = tailDirection.Value;
        }
    }
}
