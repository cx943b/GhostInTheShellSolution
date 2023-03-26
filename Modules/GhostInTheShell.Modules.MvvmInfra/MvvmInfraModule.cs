using Prism.Ioc;
using Prism.Modularity;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.MvvmInfra
{
    public class MvvmInfraModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            //throw new NotImplementedException();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDialogService, GhostDialogService>();
        }
    }
}
