using GhostInTheShell.Modules.ShellInfra;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GhostInTheShell.Modules.Shell.ViewModels
{
    internal class ShellViewModel : BindableBase, IDisposable, IDialogAware
    {
        readonly MaterialCollectionChangedEvent _matCollChangedEvent;
        readonly ShellSizeChangedEvent _sizeChangedEvent;

        readonly SubscriptionToken _subMatCollToken;
        readonly SubscriptionToken _subSizeToken;

        BitmapImage? _ImageSource;
        Size _ImageSize;
        double _Left = 400, _Top;
        double _Width, _Height;

        public event Action<IDialogResult> RequestClose = null!;

        public BitmapImage? ImageSource
        {
            get => _ImageSource;
            set => SetProperty(ref _ImageSource, value);
        }
        public Size ImageSize
        {
            get => _ImageSize;
            set => SetProperty(ref _ImageSize, value);
        }

        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
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
        public string Title { get; set; } = "Shell";

        public ShellViewModel(IEventAggregator eventAggregator)
        {
            _matCollChangedEvent = eventAggregator.GetEvent<MaterialCollectionChangedEvent>();
            _sizeChangedEvent = eventAggregator.GetEvent<ShellSizeChangedEvent>();

            _subMatCollToken = _matCollChangedEvent.Subscribe(onMaterialCollectionChanged);
            _subSizeToken = _sizeChangedEvent.Subscribe(onShellSizeChanged);
        }

        public void Dispose()
        {
            _subMatCollToken.Dispose();
            _subSizeToken.Dispose();
        }

        

        private void onShellSizeChanged(System.Drawing.Size shellSize)
        {
            Width = shellSize.Width;
            Height = shellSize.Height;
            Left = SystemParameters.WorkArea.Width - shellSize.Width;
            Top = SystemParameters.WorkArea.Height - shellSize.Height;

            ImageSize = new Size(_Width, _Height);
        }

        private void onMaterialCollectionChanged(MemoryStream imgStream)
        {
            if(_ImageSource is not null)
                _ImageSource.StreamSource.Dispose();

            if (imgStream is not null)
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = imgStream;
                bi.EndInit();

                if (bi.CanFreeze)
                    bi.Freeze();

                ImageSource = bi;
            }
            else
            {
                ImageSource = null;
            }
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue(nameof(ShellViewModel.ImageSize), out System.Drawing.Size imgSize))
            {
                onShellSizeChanged(imgSize);
            }
            else
            {
                throw new KeyNotFoundException(nameof(ShellViewModel));
            }
        }
    }
}
