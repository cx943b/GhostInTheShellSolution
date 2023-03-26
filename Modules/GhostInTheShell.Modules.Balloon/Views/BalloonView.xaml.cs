using GhostInTheShell.Modules.Balloon.ViewModels;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GhostInTheShell.Modules.Balloon.Views
{
    /// <summary>
    /// BalloonView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BalloonView : UserControl, IDialogAware
    {

        public BalloonView()
        {
            InitializeComponent();
        }

        public string Title => "BalloonWindow";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //throw new NotImplementedException();
        }
    }
}
