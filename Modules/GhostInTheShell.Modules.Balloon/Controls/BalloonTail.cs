using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    public enum BalloonTailDirection { Left, Right, Top, Bottom };

    

    public class BalloonTail : Control
    {
        public const string PanelName = "PART_Canvas";
        public const string TailPathName = "PART_TailPath";
        public const double TailOffset = 20d;

        Point _dragStartPos;
        Point _dragStartPathPos;

        Path _tailPath;
        Canvas _tailPanel;

        bool _isManualPositioning;




        public bool IsDirecting
        {
            get { return (bool)GetValue(IsDirectingProperty); }
            set { SetValue(IsDirectingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirecting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirectingProperty =
            DependencyProperty.Register("IsDirecting", typeof(bool), typeof(BalloonTail), new PropertyMetadata(false));





        public static readonly DependencyProperty TailPositionProperty =
            DependencyProperty.Register("TailPosition", typeof(double), typeof(BalloonTail), new UIPropertyMetadata(0d, onTailPositionPropertyChanged));
        public static readonly DependencyProperty TailDirectionProperty =
            DependencyProperty.Register("TailDirection", typeof(BalloonTailDirection), typeof(BalloonTail), new UIPropertyMetadata(BalloonTailDirection.Right));


        public BalloonTailDirection TailDirection
        {
            get { return (BalloonTailDirection)GetValue(TailDirectionProperty); }
            set { SetValue(TailDirectionProperty, value); }
        }

        /// <summary>
        /// Tail's PercentPosition
        /// </summary>
        public double TailPosition
        {
            get { return (double)GetValue(TailPositionProperty); }
            set { SetValue(TailPositionProperty, value); }
        }

        


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas? tailCanvas = GetTemplateChild(PanelName) as Canvas;
            if(tailCanvas is null)
                throw new NullReferenceException(nameof(tailCanvas));

            Path? tailPath = this.GetTemplateChild(TailPathName) as Path;
            if(tailPath is null)
                throw new NullReferenceException(nameof(tailPath));
            
            this.MouseLeftButtonDown += onMouseLeftButtonDown;
            this.MouseLeftButtonUp += onMouseLeftButtonUp;
            this.MouseMove += onMouseMove;

            _tailPanel = tailCanvas;
            _tailPath = tailPath;

            OnTailPositionChanged(0, (double)GetValue(TailPositionProperty));
        }

        protected virtual void OnTailPositionChanged(double oldPosition, double newPosition)
        {
            TailPosition = newPosition;

            if(_tailPath is not null && !_isManualPositioning)
            {
                BalloonTailDirection tailDirection = TailDirection;

                if (tailDirection == BalloonTailDirection.Left || tailDirection == BalloonTailDirection.Right)
                {
                    double moveLength = _tailPanel.ActualHeight - _tailPath.Height;
                    Canvas.SetTop(_tailPath, newPosition * moveLength / 100);
                }
                else
                {
                    double moveLength = _tailPanel.ActualWidth - _tailPath.Width;
                    Canvas.SetLeft(_tailPath, newPosition * moveLength / 100);
                }
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var finalSize = base.ArrangeOverride(arrangeBounds);

            BalloonTailDirection tailDirection = TailDirection;
            double newPosition = TailPosition;

            if (tailDirection == BalloonTailDirection.Left || tailDirection == BalloonTailDirection.Right)
            {
                double moveLength = _tailPanel.ActualHeight - _tailPath.Height;
                double pathTop = newPosition * moveLength / 100;

                if (pathTop < 0)
                    pathTop = 0;
                else if (pathTop > moveLength)
                    pathTop = moveLength;

                Canvas.SetTop(_tailPath, pathTop);
            }
            else
            {
                double moveLength = _tailPanel.ActualWidth - _tailPath.Width;
                double pathLeft = newPosition * moveLength / 100;

                if (pathLeft < 0)
                    pathLeft = 0;
                else if (pathLeft > moveLength)
                    pathLeft = moveLength;

                Canvas.SetLeft(_tailPath, pathLeft);
            }

            return finalSize;
        }


        private static void onMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            BalloonTail ballTail = (BalloonTail)sender;
            ballTail._dragStartPos = e.GetPosition(ballTail._tailPanel);
            ballTail._dragStartPathPos = new Point(Canvas.GetLeft(ballTail._tailPath), Canvas.GetTop(ballTail._tailPath));
            ballTail.CaptureMouse();

            ballTail._isManualPositioning = true;
        }
        private static void onMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            BalloonTail ballTail = (BalloonTail)sender;
            ballTail.ReleaseMouseCapture();

            BalloonTailDirection tailDirection = ballTail.TailDirection;
            double oldPosition = 0d, newPosition = 0d;

            if (tailDirection == BalloonTailDirection.Left || tailDirection == BalloonTailDirection.Right)
            {
                double moveLength = ballTail._tailPanel.ActualHeight - ballTail._tailPath.Height;
                oldPosition = ballTail._dragStartPathPos.Y * 100 / moveLength;

                double tailTop = Canvas.GetTop(ballTail._tailPath);
                newPosition = tailTop * 100 / moveLength;
            }
            else
            {
                double moveLength = ballTail._tailPanel.Width - ballTail._tailPath.Width;
                oldPosition = ballTail._dragStartPathPos.X * 100 / moveLength;

                double tailLeft = Canvas.GetLeft(ballTail._tailPath);
                newPosition = tailLeft * 100 / moveLength;
            }

            //Console.WriteLine($"TailPos(%): {newPosition}");
            ballTail.OnTailPositionChanged(oldPosition, newPosition);

            ballTail._isManualPositioning = false;
        }
        private static void onMouseMove(object sender, MouseEventArgs e)
        {
            BalloonTail ballTail = (BalloonTail)sender;

            if(ballTail.IsMouseCaptured)
            {
                Point mousePos = e.GetPosition(ballTail._tailPanel);
                BalloonTailDirection tailDirection = ballTail.TailDirection;

                if(tailDirection == BalloonTailDirection.Left || tailDirection == BalloonTailDirection.Right)
                {
                    double dy = mousePos.Y - ballTail._dragStartPos.Y;
                    double newTop = ballTail._dragStartPathPos.Y + dy;

                    double topMax = ballTail._tailPanel.ActualHeight - ballTail._tailPath.Height;

                    if (newTop < 0)
                        newTop = 0;
                    else if (newTop > topMax)
                        newTop = topMax;

                    Canvas.SetTop(ballTail._tailPath, newTop);
                }
                else
                {
                    double dx = mousePos.X - ballTail._dragStartPos.X;
                    double newLeft = ballTail._dragStartPathPos.X + dx;

                    double leftMax = ballTail._tailPanel.ActualWidth - ballTail._tailPath.Width;

                    if (newLeft < 0)
                        newLeft = 0;
                    else if (newLeft > leftMax)
                        newLeft = leftMax;

                    Canvas.SetLeft(ballTail._tailPath, newLeft);
                }
            }
        }

        private static void onTailPositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) =>
            (depObj as BalloonTail)?.OnTailPositionChanged((double)e.OldValue, (double)e.NewValue);
   }
}
