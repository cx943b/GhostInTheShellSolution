using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Shell.Models
{

    [Serializable]
    public sealed class ColorableMaterialModel : MaterialModel, IColorable
    {
        readonly int _width, _height;

        float _h, _s, _l;
        bool _IsUseColor = false;
        Bitmap _colorBitmap;

        public override Bitmap ImageData => _IsUseColor ? _colorBitmap : _bitmap;
        public bool IsUseColor
        {
            get => _IsUseColor;
            set
            {
                if (_IsUseColor != value)
                {
                    _IsUseColor = value;

                    if (_IsUseColor)
                        applyColor();
                }
            }
        }

        public ColorableMaterialModel(MaterialID id, string fileName, Bitmap bitmap) : base(id, fileName, bitmap)
        {
            _width = bitmap.Width;
            _height = bitmap.Height;

            if (_IsUseColor)
                applyColor();
        }

        public override void Dispose()
        {
            _colorBitmap?.Dispose();
            _colorBitmap = null;

            base.Dispose();
        }

        public void ChangeColor(float h, float s, float l)
        {
            _h = h;
            _s = s;
            _l = l;

            _IsUseColor = true;

            applyColor();
        }
        public void ChangeDefaultColor()
        {
            _IsUseColor = false;

            _colorBitmap?.Dispose();
            _colorBitmap = null;
        }

        private void applyColor()
        {
            if (_colorBitmap == null)
            {
                _colorBitmap = new Bitmap(_width, _height);
                _colorBitmap.SetResolution(96.0f, 96.0f);
            }


            Rectangle rect = new Rectangle(0, 0, _width, _height);
            BitmapData bitData = _bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData colorBitData = _colorBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //ImageProcessing.HslProcess(bitData, colorBitData, _h, _s, _l);

            _colorBitmap.UnlockBits(colorBitData);
            _bitmap.UnlockBits(bitData);
        }
    }
}
