using GhostInTheShell.Modules.Shell.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GhostInTheShell.Modules.Shell.ViewModels
{
    public class ShellSizeChangedEvent : PubSubEvent<Size>
    {

    }
    public class MaterialCollectionChangedEvent : PubSubEvent<MemoryStream>
    {

    }

    internal class ShellViewModel : BindableBase, IDisposable
    {
        Size _ShellSize = new Size();
        BitmapImage? _ShellSource;

        readonly SubscriptionToken _materialSubToken;
        readonly SubscriptionToken _sizeSubToken;

        public BitmapImage? ShellSource
        {
            get => _ShellSource;
            internal set => SetProperty(ref _ShellSource, value);
        }
        public Size ShellSize
        {
            get => _ShellSize;
            internal set => SetProperty(ref _ShellSize, value);
        }

        public ShellViewModel(IEventAggregator eventAggregator)
        {
            _materialSubToken = eventAggregator.GetEvent<MaterialCollectionChangedEvent>().Subscribe(OnMaterialCollectionChanged);
            _sizeSubToken = eventAggregator.GetEvent<ShellSizeChangedEvent>().Subscribe(OnShellSizeChanged);
        }

        protected virtual void OnMaterialCollectionChanged(MemoryStream materialStream)
        {
            if(_ShellSource is not null)
            {
                _ShellSource.StreamSource.Dispose();
            }

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource= materialStream;
            bi.EndInit();

            ShellSource = bi;
        }
        protected virtual void OnShellSizeChanged(Size shellSize)
        {
            ShellSize = shellSize;
        }

        public void Dispose()
        {
            _materialSubToken?.Dispose();
            _sizeSubToken?.Dispose();
        }
    }
}
