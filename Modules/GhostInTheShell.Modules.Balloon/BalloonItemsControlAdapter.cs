using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GhostInTheShell.Modules.Balloon.Controls;
using GhostInTheShell.Modules.Balloon.Models;
using Prism.Regions;

namespace GhostInTheShell.Modules.Balloon
{
    public class BalloonItemsControlRegion : AllActiveRegion
    {
        public Size Size { get; set; } = new Size(Double.NaN, Double.NaN);
    }


    public class BalloonItemsControlAdapter : RegionAdapterBase<BalloonItemsControl>
    {
        BalloonItemsControl? _regionTarget;

        public BalloonItemsControlAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, BalloonItemsControl regionTarget)
        {
            _regionTarget = regionTarget;
            _regionTarget.SizeChanged += (s, e) => ((BalloonItemsControlRegion)region).Size = e.NewSize;
            region.Views.CollectionChanged += onRegionCollectionChanged;
        }


        protected override IRegion CreateRegion() => new BalloonItemsControlRegion();

        private void onRegionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Size regionTargetSize = _regionTarget!.DesiredSize;
            var viewColl = (ViewsCollection)sender!;

            int contentCount = viewColl.Count();
            //var befLastContent = viewColl.ElementAt(contentCount - 2);


            if (e.Action== NotifyCollectionChangedAction.Add)
            {
                BalloonContentControlBase? contentCtrl = null;
                foreach(var newItem in e.NewItems)
                {
                    contentCtrl = null;

                    switch (newItem)
                    {
                        case BalloonTextConetntModel txtContent:
                            {
                                contentCtrl = new BalloonTextContentControl();
                                break;
                            }
                        case BalloonImageContentModel imgContent:
                            {
                                contentCtrl = new BalloonImageContentControl();
                                break;
                            }
                    }

                    if(contentCtrl is null)
                    {
                        throw new InvalidCastException("NotBalloonContentDataContext");
                    }

                    contentCtrl.DataContext = newItem;

                    Console.WriteLine($"BefAddHeight: {_regionTarget.ActualHeight}");

                    _regionTarget!.Items.Add(contentCtrl);

                    Console.WriteLine($"BefAddHeight: {_regionTarget.ActualHeight}");

                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    foreach(FrameworkElement child in _regionTarget.Items)
                    {
                        if(child.DataContext == oldItem)
                        {
                            _regionTarget.Items.Remove(child);
                            break;
                        }
                    }
                }
            }
            //else if (e.Action == NotifyCollectionChangedAction.Reset)
            //{
            //    _regionTarget.Items.Clear();
            //}
        }
    }
}
