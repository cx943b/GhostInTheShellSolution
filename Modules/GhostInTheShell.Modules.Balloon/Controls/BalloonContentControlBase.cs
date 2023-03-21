using System.Windows;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    internal abstract class BalloonContentControlBase : FrameworkElement, IBalloonContent
    {
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(BalloonContentControlBase), new UIPropertyMetadata(new Point(), onPositionPropertyChanged));
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Size), typeof(BalloonContentControlBase), new UIPropertyMetadata(new Size(), onSizePropertyChanged));

        protected virtual void onPositionChanged(Point oldPos, Point newPos)
        {
            //Canvas.SetLeft(this, newPos.X);
            //Canvas.SetTop(this, newPos.Y);
        }
        protected virtual void onSizeChanged(Size oldSize, Size newSize)
        {
            //Width = newSize.Width;
            //Height = newSize.Height;
        }

        private static void onPositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonTextContentControl? txtContent = depObj as BalloonTextContentControl;
            if (txtContent != null)
                txtContent.onPositionChanged((Point)e.OldValue, (Point)e.NewValue);
        }
        private static void onSizePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonTextContentControl? txtContent = depObj as BalloonTextContentControl;
            if (txtContent != null)
                txtContent.onSizeChanged((Size)e.OldValue, (Size)e.NewValue);
        }
    }
}
