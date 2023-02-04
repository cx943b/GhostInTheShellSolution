using GhostInTheShell.Modules.ShellInfra;
using Prism.Events;
using Prism.Mvvm;
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
    internal sealed class ShellViewModel : BindableBase, IDisposable
    {
        readonly MaterialCollectionChangedEvent _matCollChangedEvent;
        readonly ShellSizeChangedEvent _sizeChangedEvent;

        readonly SubscriptionToken _subMatCollToken;
        readonly SubscriptionToken _subSizeToken;

        BitmapImage? _ImageSource;
        Size _ImageSize;

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
            ImageSize = new Size(shellSize.Width, shellSize.Height);
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
    }
}
