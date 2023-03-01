using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Prism.Mvvm;

namespace GhostInTheShell.Modules.Balloon.Models
{
    public abstract class BalloonContentModelBase : BindableBase
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

        public double Right => _Position.X + _Size.Width;
        public double Bottom => _Position.Y + _Size.Height;
    }

    public class BalloonTextConetntModel : BalloonContentModelBase
    {
        string? _Text;

        public string? Text
        {
            get => _Text;
            set => SetProperty(ref _Text, value);
        }
    }
}
