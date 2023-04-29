using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    internal class BalloonImageContentControl : BalloonContentControlBase
    {
        BitmapImage? _imgCache;
        Size _imgSizeCache;

        public static readonly DependencyProperty ImageHorizontalAlignmentProperty =
            DependencyProperty.Register("ImageHorizontalAlignment", typeof(HorizontalAlignment), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register("ImageSize", typeof(Size), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(Uri), typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, onImageUriPropertyChanged));

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

        static BalloonImageContentControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BalloonImageContentControl), new FrameworkPropertyMetadata(typeof(BalloonImageContentControl)));

        protected virtual void OnImageUriChanged(Uri newUri)
        {
            if(newUri is null)
            {
                _imgCache!.StreamSource?.Dispose();
                _imgCache = null;
                return;
            }

            _imgCache = new BitmapImage();
            _imgCache.BeginInit();
            _imgCache.UriSource = newUri;
            _imgCache.EndInit();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_imgCache is null)
                return;

            Point pos = new Point(0,0);

            HorizontalAlignment hAlignment = ImageHorizontalAlignment;

            if (RenderSize.Width > _imgSizeCache.Width)
            {
                if (hAlignment == HorizontalAlignment.Right)
                {
                    pos.X = RenderSize.Width - _imgSizeCache.Width;
                }
                else if (hAlignment == HorizontalAlignment.Center)
                {
                    pos.X = (RenderSize.Width - _imgSizeCache.Width) / 2;
                }
            }
            
            dc.DrawImage(_imgCache, new Rect(pos, _imgSizeCache));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_imgCache is null)
                return new Size();

            Size imgSize = ImageSize;

            if(imgSize.Height > 40)
            {
                double ratio = 40 / imgSize.Height;
                _imgSizeCache = new Size(imgSize.Width * ratio, 40);

                return _imgSizeCache;
            }
            else
            {
                _imgSizeCache = imgSize;
            }

            return imgSize;
        }

        private static void onImageUriPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) => ((BalloonImageContentControl)depObj).OnImageUriChanged((Uri)e.NewValue);
    }
}
