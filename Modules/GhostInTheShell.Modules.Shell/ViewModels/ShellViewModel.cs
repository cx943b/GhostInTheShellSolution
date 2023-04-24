using GhostInTheShell.Modules.MvvmInfra;
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
    internal class ShellViewModel : GhostViewModelBase, IDisposable
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

        protected override void OnDialogOpenedBase(IDialogParameters parameters)
        {
            base.OnDialogOpenedBase(parameters);

            if (parameters.TryGetValue(nameof(ShellViewModel.ImageSize), out System.Drawing.Size imgSize))
                onShellSizeChanged(imgSize);
        }

        protected override bool IsTarget(string identifier)
        {
            if (String.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            return String.Equals(identifier, this.Identifier, StringComparison.OrdinalIgnoreCase);
        }

        private void onShellSizeChanged(ShellSizeChangedEventArgs e)
        {
            if (!base.IsTarget(e.Identifier))
                return;

            System.Drawing.Size size = e.Size;
            onShellSizeChanged(size);
        }
        private void onShellSizeChanged(System.Drawing.Size size)
        {
            Width = size.Width;
            Height = size.Height;
            Left = SystemParameters.WorkArea.Width - size.Width;
            Top = SystemParameters.WorkArea.Height - size.Height;

            ImageSize = new Size(Width, Height);
        }

        private void onMaterialCollectionChanged(MaterialCollectionChangedEventArgs e)
        {
            if (!this.IsTarget(e.Identifier))
                return;

            if (_ImageSource is not null)
                _ImageSource.StreamSource.Dispose();

            if (e.Stream is not null)
            {
                e.Stream.Position = 0;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = e.Stream;
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
