using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Balloon.Models;
using Prism.Commands;
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
    public class BalloonViewModel : BindableBase, IDialogAware
    {
        BalloonTailDirection _TailDirection = BalloonTailDirection.Right;
        double _TailPosition = 50;
        double _Width = 400;

        readonly DelegateCommand<BalloonTailDirection?> _TailDirectionSelectedCommand;
        readonly DelegateCommand<MouseButtonEventArgs> _MouseLeftButtonDownCommand;
        readonly DelegateCommand<MouseButtonEventArgs> _MouseLeftButtonUpCommand;
        readonly DelegateCommand<MouseEventArgs> _MouseMoveCommand;

        public event Action<IDialogResult> RequestClose;

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

        public ICommand TailDirectionSelectedCommand => _TailDirectionSelectedCommand;
        public ICommand MouseLeftButtonDownCommand => _MouseLeftButtonDownCommand;
        public ICommand MouseLeftButtonUpCommand => _MouseLeftButtonUpCommand;
        public ICommand MouseMoveCommand => _MouseMoveCommand;

        public string Title => "BalloonDialog";

        public BalloonViewModel()
        {
            _TailDirectionSelectedCommand = new DelegateCommand<BalloonTailDirection?>(onBalloonTailDirectionSelectedExecute);

            _MouseLeftButtonDownCommand = new DelegateCommand<MouseButtonEventArgs>(onMouseLeftButtonDown);
            _MouseLeftButtonUpCommand = new DelegateCommand<MouseButtonEventArgs>(onMouseLeftButtonUp);
            _MouseMoveCommand = new DelegateCommand<MouseEventArgs>(onMouseMove);
        }

        private void onMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Console.WriteLine(e);
        }
        private void onMouseLeftButtonUp(MouseButtonEventArgs e)
        {

        }
        private void onMouseMove(MouseEventArgs e)
        {

        }




        private void onBalloonTailDirectionSelectedExecute(BalloonTailDirection? tailDirection)
        {
            TailDirection = tailDirection!.Value;
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
    }
}
