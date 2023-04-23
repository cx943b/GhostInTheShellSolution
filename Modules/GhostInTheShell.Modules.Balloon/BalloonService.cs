using GhostInTheShell.Modules.Balloon.Models;
using GhostInTheShell.Modules.InfraStructure;
using Microsoft.Extensions.Logging;
using Prism.Events;
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
        
    internal class BalloonService : IBalloonService
    {
        readonly ILogger _logger;

        readonly BalloonTextContentAddedEvent _balloonTextContentAddedEvent;
        readonly BalloonImageContentAddedEvent _balloonImageContentAddedEvent;
        readonly BalloonClearedEvent _balloonClearedEvent;
        readonly BalloonPositionChangeEvent _balloonPositionChangeEvent;

        public BalloonService(ILogger<BalloonService> logger, IEventAggregator eventAggregator)
        {
            _logger = logger;

            _balloonTextContentAddedEvent = eventAggregator.GetEvent<BalloonTextContentAddedEvent>();
            _balloonImageContentAddedEvent = eventAggregator.GetEvent<BalloonImageContentAddedEvent>();
            _balloonClearedEvent = eventAggregator.GetEvent<BalloonClearedEvent>();
            _balloonPositionChangeEvent = eventAggregator.GetEvent<BalloonPositionChangeEvent>();
        }


        public void AddText(string charName, string text)
        {
            if(String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(charName)}");
                return;
            }
            else if(String.IsNullOrEmpty(text))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(text)}");
                return;
            }

            _balloonTextContentAddedEvent.Publish(new BalloonTextContentAddedEventArgs(charName, text));
        }
        public void AddImage(string charName, Uri imgUri, Size imgSize)
        {
            if (String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(charName)}");
                return;
            }
            else if (imgUri is null)
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(imgUri)}");
                return;
            }

            _balloonImageContentAddedEvent.Publish(new BalloonImageContentAddedEventArgs(charName, imgUri, imgSize));
        }

        public void ChangePosition(string charName, Point pos)
        {
            if (String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(charName)}");
                return;
            }

            _balloonPositionChangeEvent.Publish(new BalloonPositionChangeEventArgs(charName, pos));
        }
        public void Clear(string charName)
        {
            if (String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullOrEmpty: {nameof(charName)}");
                return;
            }

            _balloonClearedEvent.Publish(new BalloonClearedEventArgs(charName));
        }
    }
}