using GhostInTheShell.Modules.Balloon.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class BalloonTests
    {
        [WpfTestMethod]
        public void ShowTest()
        {
            BalloonView view = new BalloonView();

            Window w = new Window();
            w.Closed += (s, e) => Dispatcher.CurrentDispatcher.InvokeShutdown();
            w.Content = view;
            w.Show();

            Dispatcher.Run();
        }
    }
}
