using GhostInTheShell.Modules.Balloon.Models;
using GhostInTheShell.Modules.InfraStructure;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GhostInTheShell.Modules.Balloon
{
    public class BalloonService : IBalloonService
    {
        IRegionManager _regionMgr;
        IRegion _balloonContentsRegion;
        readonly ILogger _logger;

        readonly List<BalloonContentModelBase> _lstContent = new List<BalloonContentModelBase>();

        public BalloonService(ILogger<BalloonService> logger, IRegionManager regionManager)
        {
            _logger = logger;
            _regionMgr = regionManager;
        }

        public void AddText(string text)
        {
            if (_balloonContentsRegion is null)
                _balloonContentsRegion = _regionMgr.Regions[WellknownRegionNames.BalloonContentRegion];
            if (string.IsNullOrEmpty(text))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(text)}");
                return;
            }

            BalloonTextConetntModel textContent = new BalloonTextConetntModel();
            textContent.Text = text;

            var lastContent = _lstContent.LastOrDefault();
            double nextX = 0d;

            if (lastContent != null)
            {
                nextX = lastContent.Right;
            }
            
            textContent.Position = new Point(nextX, 0);

            _balloonContentsRegion.Add(textContent);
            _lstContent.Add(textContent);
        }
    }
}