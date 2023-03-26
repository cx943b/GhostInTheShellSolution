using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GhostInTheShell.Modules.MvvmInfra.Controls
{
    public enum GhostDialogWindowMoveDirection { All, Horizontal, Vertical }
    public class GhostDialogWindow : Window, IDialogWindow
    {
        const int MoveStartLength = 30;

        public static readonly DependencyProperty PositionYProperty = DependencyProperty.Register("PositionY", typeof(double), typeof(GhostDialogWindow), new PropertyMetadata(0d, onPositionYPropertyChanged));
        public static readonly DependencyProperty PositionXProperty = DependencyProperty.Register("PositionX", typeof(double), typeof(GhostDialogWindow), new PropertyMetadata(0d, onPositionXPropertyChanged));
        public static readonly DependencyProperty SizeWProperty = DependencyProperty.Register("SizeW", typeof(double), typeof(GhostDialogWindow), new PropertyMetadata(0d, onSizeWPropertyChanged));
        public static readonly DependencyProperty SizeHProperty = DependencyProperty.Register("SizeH", typeof(double), typeof(GhostDialogWindow), new PropertyMetadata(0d, onSizeHPropertyChanged));

        Point _dragStartPos;
        Point _dragStartMousePos;

        public double PositionX
        {
            get { return (double)GetValue(PositionXProperty); }
            set { SetValue(PositionXProperty, value); }
        }
        public double PositionY
        {
            get { return (double)GetValue(PositionYProperty); }
            set { SetValue(PositionYProperty, value); }
        }
        public double SizeW
        {
            get { return (double)GetValue(SizeWProperty); }
            set { SetValue(SizeWProperty, value); }
        }
        public double SizeH
        {
            get { return (double)GetValue(SizeHProperty); }
            set { SetValue(SizeHProperty, value); }
        }

        public GhostDialogWindowMoveDirection MoveDirection { get; set; }

        protected Point DragStartPos => _dragStartPos;
        protected Point DragStartMousePos => _dragStartMousePos;

        public IDialogResult Result { get; set; } = null!;

        public GhostDialogWindow()
        {
            AllowsTransparency = true;
            Background = null;
            WindowStyle = WindowStyle.None;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            _dragStartMousePos = PointToScreen(e.GetPosition(this));
            _dragStartPos = new Point(PositionX, PositionY);

            CaptureMouse();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseCaptured)
            {
                Point mousePos = PointToScreen(e.GetPosition(this));
                Vector v = mousePos - _dragStartMousePos;

                if (v.Length > MoveStartLength)
                {
                    if (MoveDirection == GhostDialogWindowMoveDirection.All)
                    {
                        moveHorizontal(mousePos);
                        moveVertical(mousePos);
                    }
                    else if (MoveDirection == GhostDialogWindowMoveDirection.Horizontal)
                    {
                        moveHorizontal(mousePos);
                    }
                    else
                    {
                        moveVertical(mousePos);
                    }
                }
            }
        }

        private void moveHorizontal(Point mousePos)
        {
            double dx = mousePos.X - _dragStartMousePos.X;
            double maxLeft = SystemParameters.WorkArea.Width - Width;
            double left = _dragStartPos.X + dx;

            if (left < 0)
                left = 0;
            else if (left > maxLeft)
                left = maxLeft;

            PositionX = left;
        }
        private void moveVertical(Point mousePos)
        {
            double dy = mousePos.Y - _dragStartMousePos.Y;
            PositionY = _dragStartPos.Y + dy;
        }

        private static void onPositionXPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) => ((Window)depObj).Left = (double)e.NewValue;
        private static void onPositionYPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) => ((Window)depObj).Top = (double)e.NewValue;
        private static void onSizeWPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) => ((Window)depObj).Width = (double)e.NewValue;
        private static void onSizeHPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) => ((Window)depObj).Height = (double)e.NewValue;
    }
}
