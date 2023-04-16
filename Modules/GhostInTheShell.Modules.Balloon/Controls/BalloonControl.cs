using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    internal class BalloonControl : ContentControl
    {
        const string TailBaseName = "PART_Tail";

        public static readonly DependencyProperty TailDirectionProperty = DependencyProperty.Register(
            "TailDirection", typeof(BalloonTailDirection), typeof(BalloonControl), new UIPropertyMetadata(BalloonTailDirection.Right, onBalloonTailDirectionPropertyChanged));
        public static readonly DependencyProperty TailPositionProperty = DependencyProperty.Register(
            "TailPosition", typeof(double), typeof(BalloonControl), new UIPropertyMetadata(50d));

        public double TailPosition
        {
            get { return (double)GetValue(TailPositionProperty); }
            set { SetValue(TailPositionProperty, value); }
        }
        public BalloonTailDirection TailDirection
        {
            get { return (BalloonTailDirection)GetValue(TailDirectionProperty); }
            set { SetValue(TailDirectionProperty, value); }
        }

        

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);

            double targetHeight = child.DesiredSize.Height + 48;

            DoubleAnimation daHeight = new DoubleAnimation();
            daHeight.DecelerationRatio = 1;
            daHeight.Duration = TimeSpan.FromMilliseconds(200);
            daHeight.To = targetHeight > MinHeight ? targetHeight : MinHeight;

            this.BeginAnimation(HeightProperty, daHeight);
        }

        private static void onBalloonTailDirectionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonControl ballCtrl = (BalloonControl)depObj;

            BalloonTailDirection oldDirection = (BalloonTailDirection)e.OldValue;
            BalloonTail? oldTail = ballCtrl.GetTemplateChild(TailBaseName + oldDirection.ToString()) as BalloonTail;

            if(oldTail != null)
                oldTail.IsDirecting = false;

            BalloonTailDirection newDirection = (BalloonTailDirection)e.NewValue;
            BalloonTail? newTail = ballCtrl.GetTemplateChild(TailBaseName + newDirection.ToString()) as BalloonTail;

            if (newTail != null)
                newTail.IsDirecting = true;
        }

    }
}
