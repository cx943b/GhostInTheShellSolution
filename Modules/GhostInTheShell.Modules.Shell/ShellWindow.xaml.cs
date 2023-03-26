using GhostInTheShell.Modules.MvvmInfra.Controls;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ShellWindow : GhostDialogWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
            MoveDirection = GhostDialogWindowMoveDirection.Horizontal;
        }
    }
}
