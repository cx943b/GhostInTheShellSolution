using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GhostInTheShell.Modules.Balloon.ViewModels
{
    public class BalloonItemViewModel : BindableBase
    {
        Point _Position;
        Size _Size;

        public Point Position
        {
            get => _Position;
            set => SetProperty(ref _Position, value);
        }
        public Size Size
        {
            get => _Size;
            set => SetProperty(ref _Size, value);
        }
    }
}
