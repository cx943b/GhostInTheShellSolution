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
using System.Windows.Media.Imaging;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    public interface IBalloonContent
    {
        public Point Position { get; set; }
        public Size Size { get; set; }
    }

    internal abstract class BalloonContentControlBase : FrameworkElement, IBalloonContent
    {
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(BalloonContentControlBase), new UIPropertyMetadata(new Point(), onPositionPropertyChanged));
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Size), typeof(BalloonContentControlBase), new UIPropertyMetadata(new Size(), onSizePropertyChanged));

        protected virtual void onPositionChanged(Point oldPos, Point newPos)
        {
            //Canvas.SetLeft(this, newPos.X);
            //Canvas.SetTop(this, newPos.Y);
        }
        protected virtual void onSizeChanged(Size oldSize, Size newSize)
        {
            //Width = newSize.Width;
            //Height = newSize.Height;
        }

        private static void onPositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonTextContentControl? txtContent = depObj as BalloonTextContentControl;
            if (txtContent != null)
                txtContent.onPositionChanged((Point)e.OldValue, (Point)e.NewValue);
        }
        private static void onSizePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BalloonTextContentControl? txtContent = depObj as BalloonTextContentControl;
            if (txtContent != null)
                txtContent.onSizeChanged((Size)e.OldValue, (Size)e.NewValue);
        }
    }

    internal class BalloonImageContentControl : BalloonContentControlBase
    {
        public static readonly DependencyProperty ImageHorizontalAlignmentProperty =
            DependencyProperty.Register("ImageHorizontalAlignment", typeof(HorizontalAlignment), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register("ImageSize", typeof(Size), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(Uri), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public HorizontalAlignment ImageHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(ImageHorizontalAlignmentProperty); }
            set { SetValue(ImageHorizontalAlignmentProperty, value); }
        }
        public Size ImageSize
        {
            get { return (Size)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }
        public Uri ImageUri
        {
            get { return (Uri)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ImageUri is null)
                return;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = ImageUri;
            bi.DecodePixelHeight = (int)ImageSize.Height;
            bi.EndInit();

            

            if (bi.CanFreeze)
                bi.Freeze();

            Point pos = new Point(20,0);
            Size imgSize = ImageSize;

            HorizontalAlignment hAlignment = ImageHorizontalAlignment;

            if (RenderSize.Width > imgSize.Width)
            {
                if (hAlignment == HorizontalAlignment.Right)
                {
                    pos.X = RenderSize.Width - imgSize.Width;
                }
                else if (hAlignment == HorizontalAlignment.Center)
                {
                    pos.X = (RenderSize.Width - imgSize.Width) / 2;
                }
            }
            
            dc.DrawImage(bi, new Rect(pos, imgSize));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize) => ImageSize;
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
