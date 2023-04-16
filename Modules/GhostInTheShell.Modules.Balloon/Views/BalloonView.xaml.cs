using GhostInTheShell.Modules.Balloon.ViewModels;
using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.MvvmInfra;
using Microsoft.Extensions.Configuration;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    internal partial class BalloonView : UserControl, IDialogAware
    {
        public BalloonView(IConfiguration config)
        {
            InitializeComponent();

            var vm = DataContext as IGhostIdentifier;
            vm!.IdentifierChanged += Vm_IdentifierChanged;
        }

        private void Vm_IdentifierChanged(object sender, GhostIdentifierChangedEventArgs e)
        {
            ContentControl contentCtrl = new ContentControl();
            RegionManager.SetRegionName(contentCtrl, e.Identifier + WellknownRegionNames.BalloonContentViewRegion);

            ballCtrl.Content = contentCtrl;
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
