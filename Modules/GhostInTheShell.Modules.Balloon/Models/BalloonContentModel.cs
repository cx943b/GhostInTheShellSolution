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
        double _Width, _Height;
        bool _IsLastInLine;

        public Point Position
        {
            get => _Position;
            set => SetProperty(ref _Position, value);
        }
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }
        public double Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }
        public bool IsLastInLine
        {
            get => _IsLastInLine;
            set => SetProperty(ref _IsLastInLine, value);
        }

        public double Right => _Position.X + _Width;
        public double Bottom => _Position.Y + _Height;
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
    public class BalloonImageContentModel : BalloonContentModelBase
    {
        Uri? _ImageUri;
        Size _ImageSize;
        HorizontalAlignment _ImageHorizontalAlignment;

        public Uri? ImageUri
        {
            get => _ImageUri;
            set => SetProperty(ref _ImageUri, value);
        }
        public Size ImageSize
        {
            get => _ImageSize;
            set => SetProperty(ref _ImageSize, value);
        }
        public HorizontalAlignment ImageHorizontalAlignment
        {
            get => _ImageHorizontalAlignment;
            set => SetProperty(ref _ImageHorizontalAlignment, value);
        }
    }
}
