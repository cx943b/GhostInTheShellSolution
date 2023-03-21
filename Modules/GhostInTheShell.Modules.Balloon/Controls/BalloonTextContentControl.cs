using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    public interface IBalloonContent
    {
        public Point Position { get; set; }
        public Size Size { get; set; }
    }

    
    internal class BalloonTextContentControl : BalloonContentControlBase
    {
        FormattedText? _ft;

        
        public string? Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set { SetValue(FontFamilyProperty, value); }
        }
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }


        
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BalloonTextContentControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure| FrameworkPropertyMetadataOptions.AffectsRender, onTextPropertyChanged));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(BalloonTextContentControl));
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(BalloonTextContentControl));
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(BalloonTextContentControl));
        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(BalloonTextContentControl));
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(BalloonTextContentControl));
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(BalloonTextContentControl));

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawText(_ft, new Point());
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_ft is null)
                return new Size();

            var size = new Size(_ft.Width, _ft.Height);

            Size = size;
            return size;
        }

        
        protected virtual void onTextChanged(string? oldText, string? newText)
        {
            refreshFormattedText(newText);
        }
        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            
            refreshFormattedText();
            InvalidateMeasure();
        }

        private void refreshFormattedText() => refreshFormattedText(Text);
        private void refreshFormattedText(string? text)
        {
            if(String.IsNullOrEmpty(text))
            {
                _ft = null;
                return;
            }

            _ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        
        private static void onTextPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonTextContentControl? txtContent = depObj as BalloonTextContentControl;
            if (txtContent != null)
                txtContent.onTextChanged(e.OldValue?.ToString(), e.NewValue?.ToString());
        }
    }
}
