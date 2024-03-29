﻿using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GhostInTheShell.Modules.MvvmInfra
{
    public class GhostDialogService : DialogService
    {
        readonly IRegionManager _regionMgr;

        public GhostDialogService(IContainerExtension containerExtension) : base(containerExtension)
        {
            _regionMgr = containerExtension.Resolve<IRegionManager>();
        }

        protected override void ConfigureDialogWindowContent(string dialogName, IDialogWindow window, IDialogParameters parameters)
        {
            
            RegionManager.SetRegionManager((Window)window, _regionMgr);
            base.ConfigureDialogWindowContent(dialogName, window, parameters);

            ((Window)window).Owner = Application.Current.MainWindow;
        }
    }
}
