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
    public class BalloonItemsControlAdapter : RegionAdapterBase<BalloonItemsControl>
    {
        BalloonItemsControl? _regionTarget;

        public BalloonItemsControlAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, BalloonItemsControl regionTarget)
        {
            _regionTarget = regionTarget;
            region.ActiveViews.CollectionChanged += onRegionCollectionChanged;
        }


        protected override IRegion CreateRegion() => new AllActiveRegion();

        private void onRegionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action== NotifyCollectionChangedAction.Add)
            {
                foreach(var newItem in e.NewItems)
                {
                    switch(newItem)
                    {
                        case BalloonTextConetntModel txtContent:
                            {
                                TextBlock txt = new TextBlock();
                                txt.DataContext = txtContent;
                                txt.SizeChanged += onSizeChanged;

                                Binding bindText = new Binding();
                                bindText.Source = txtContent;
                                bindText.Path = new PropertyPath(nameof(BalloonTextConetntModel.Text));
                                
                                txt.SetBinding(TextBlock.TextProperty, bindText);

                                Canvas.SetLeft(txt, txtContent.Position.X);
                                Canvas.SetTop(txt, txtContent.Position.Y);

                                txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                txtContent.Size = txt.DesiredSize;

                                _regionTarget!.Items.Add(txt);
                                break;
                            }
                    }
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {

            }
        }

        private static void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var contentModel = ((FrameworkElement)sender).DataContext as BalloonContentModelBase;
            if (contentModel != null)
                contentModel.Size = e.NewSize;
        }
    }
}
