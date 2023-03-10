using GhostInTheShell.Modules.Balloon.Models;
using GhostInTheShell.Modules.InfraStructure;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace GhostInTheShell.Modules.Balloon
{
    public class TextCalculationParam
    {
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public FontStretch FontStretch { get; set; } = FontStretches.Normal;
        public FontFamily FontFamily { get; set; } = new FontFamily("Consolas");
        public double FontSize { get; set; } = 12d;

        public Typeface GetTypeface() => new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
    }
        
    public class BalloonService : IBalloonService
    {
        IRegionManager _regionMgr;
        BalloonItemsControlRegion _balloonContentsRegion;
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
                _balloonContentsRegion = (BalloonItemsControlRegion)_regionMgr.Regions[WellknownRegionNames.BalloonContentRegion];

            if (string.IsNullOrEmpty(text))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(text)}");
                return;
            }

            BalloonTextConetntModel textContent = new BalloonTextConetntModel();
            textContent.Text = text;

            _balloonContentsRegion.Add(textContent);
            _lstContent.Add(textContent);
        }
        public void AddImage(Uri imageUri)
        {
            if (_balloonContentsRegion is null)
                _balloonContentsRegion = (BalloonItemsControlRegion)_regionMgr.Regions[WellknownRegionNames.BalloonContentRegion];

            if(imageUri is null)
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(imageUri)}");
                return;
            }

            BalloonImageContentModel imgContent = new BalloonImageContentModel();
            imgContent.ImageUri = imageUri;
            imgContent.ImageSize = new Size(40, 40);

            _balloonContentsRegion.Add(imgContent);
            _lstContent.Add(imgContent);
        }
    }
}