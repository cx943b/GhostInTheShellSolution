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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace GhostInTheShell.Modules.Balloon.ViewModels
{
    internal class BalloonViewModel : GhostViewModelBase, IDialogAware, IDisposable
    {
        BalloonTailDirection _TailDirection = BalloonTailDirection.Right;

        double _TailPosition = 50;

        readonly CollectionViewSource _contentSource;
        readonly ObservableCollection<BalloonContentModelBase> _lstContentModel = new ObservableCollection<BalloonContentModelBase>();
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

        public ICollectionView Contents => _contentSource.View;

        public BalloonViewModel(IEventAggregator eventAggregator, IBalloonService ballSvc)
        {
            _contentSource = new CollectionViewSource();
            _contentSource.Source = _lstContentModel;


            _lstEventToken.Add(eventAggregator.GetEvent<BalloonImageContentAddedEvent>().Subscribe(onBalloonImageContentAdded));
            _lstEventToken.Add(eventAggregator.GetEvent<BalloonTextContentAddedEvent>().Subscribe(onBalloonTextContentAdded));
            _lstEventToken.Add(eventAggregator.GetEvent<BalloonTailDirectionChangedEvent>().Subscribe(onBalloonTailDirectionChanged));

            Width = 400;
            Height = 250;
        }

        public void Dispose()
        {
            _lstEventToken.ForEach(token => token.Dispose());
            _lstEventToken.Clear();
        }

        private void onBalloonImageContentAdded(BalloonImageContentAddedEventArgs e)
        {
            if (!base.IsTarget(e.Identifier))
                return;

            BalloonImageContentModel imgModel = new BalloonImageContentModel();
            imgModel.ImageUri = e.ImageUri;
            imgModel.ImageSize = e.ImageSize;
            imgModel.ImageHorizontalAlignment = e.HorizontalAlignment;

            _lstContentModel.Add(imgModel);
        }
        private void onBalloonTextContentAdded(BalloonTextContentAddedEventArgs e)
        {
            if (!base.IsTarget(e.Identifier))
                return;

            BalloonTextConetntModel txtModel = new BalloonTextConetntModel();
            txtModel.Text = e.Text;
            
            _lstContentModel.Add(txtModel);
        }
        private void onBalloonTailDirectionChanged(BalloonTailDirectionChangeEventArgs e)
        {
            if (!base.IsTarget(e.Identifier))
                return;

            TailDirection = e.TailDirection;
        }
    }
}
