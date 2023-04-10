using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Balloon.Models;
using GhostInTheShell.Modules.MvvmInfra;
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
    public class BalloonViewModel : GhostViewModelBase, IDialogAware, IDisposable
    {
        BalloonTailDirection _TailDirection = BalloonTailDirection.Right;

        double _TailPosition = 50;

        readonly List<SubscriptionToken> _lstEventToken = new List<SubscriptionToken>();

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

        public BalloonViewModel(IEventAggregator eventAggregator)
        {
            _lstEventToken.Add(eventAggregator.GetEvent<BalloonTailDirectionChangeEvent>().Subscribe(onBalloonTailDirectionChanged));

            Width = 400;
            Height = 250;
              
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
