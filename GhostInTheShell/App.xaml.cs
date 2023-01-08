using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;

namespace GhostInTheShell
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            Window w = new Window();
            w.Background = null;
            w.AllowsTransparency= true;
            w.WindowStyle = WindowStyle.None;
            w.ShowInTaskbar = false;

            w.Width = 200;
            w.Height = 200;

            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            throw new NotImplementedException();
        }
    }
}
