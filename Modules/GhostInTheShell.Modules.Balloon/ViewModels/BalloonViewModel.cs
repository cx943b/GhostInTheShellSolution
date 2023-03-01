using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Balloon.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GhostInTheShell.Modules.Balloon.ViewModels
{
    public class BalloonViewModel : BindableBase
    {
        BalloonTailDirection _TailDirection;

        readonly DelegateCommand<BalloonTailDirection?> _TailDirectionSelectedCommand;

        public BalloonTailDirection TailDirection
        {
            get => _TailDirection;
            set => SetProperty(ref _TailDirection, value);
        }

        public ICommand TailDirectionSelectedCommand => _TailDirectionSelectedCommand;

        public BalloonViewModel()
        {
            _TailDirectionSelectedCommand = new DelegateCommand<BalloonTailDirection?>(onBalloonTailDirectionSelectedExecute);
        }

        private void onBalloonTailDirectionSelectedExecute(BalloonTailDirection? tailDirection)
        {
            TailDirection = tailDirection!.Value;
        }
    }
}
