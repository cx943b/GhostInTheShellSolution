using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    public class BalloonItem : ContentControl
    {
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(BalloonItem), new UIPropertyMetadata(new Point(), onPositionPropertyChanged));


        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        protected virtual void OnPositionChanged(Point oldPos, Point newPos)
        {
            Canvas.SetLeft(this, newPos.X);
            Canvas.SetTop(this, newPos.Y);
        }

        private static void onPositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) =>
            (depObj as BalloonItem)?.OnPositionChanged((Point)e.OldValue, (Point)e.NewValue);
    }
}
