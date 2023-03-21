using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GhostInTheShell.Modules.Balloon
{
    /// <summary>
    /// BalloonWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BalloonWindow : Window, IDialogWindow
    {
        Point _dragStartMousePos;
        Point _dragStartWindowPos;

        public BalloonWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; } = new DialogResult();


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            _dragStartMousePos = PointToScreen(e.GetPosition(this));
            _dragStartWindowPos = new Point(Left, Top);

            Height = ((Control)Content).DesiredSize.Height + 20;

            CaptureMouse();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();

                Height = SystemParameters.WorkArea.Height - Top;
            }
                
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseCaptured)
            {
                Point mousePos = PointToScreen(e.GetPosition(this));

                double dx = mousePos.X - _dragStartMousePos.X;
                double dy = mousePos.Y - _dragStartMousePos.Y;

                Left = _dragStartWindowPos.X + dx;
                Top = _dragStartWindowPos.Y + dy;
            }
        }
    }
}
