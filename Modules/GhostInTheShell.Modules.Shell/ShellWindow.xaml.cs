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
using System.Windows.Shapes;

namespace GhostInTheShell.Modules.Shell
{
    /// <summary>
    /// ShellWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShellWindow : Window, IDialogWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
