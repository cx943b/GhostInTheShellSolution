#define Version1

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GhostInTheShell.Modules.InfraStructure
{
    public class Hsl
    {
        double _H, _S, _L;

        public double H
        {
            get => _H;
            set
            {
                double h = value;
                if (h < 0 || h > 1)
                    throw new ArgumentOutOfRangeException(nameof(h));

                _H = h;
            }
        }
        public double S
        {
            get => _S;
            set
            {
                double s = value;
                if (s < 0 || s > 1)
                    throw new ArgumentOutOfRangeException(nameof(s));
                
                _S = s;
            }
        }
        public double L
        {
            get => _L;
            set
            {
                double l = value;
                if (l < 0 || l > 1)
                    throw new ArgumentOutOfRangeException(nameof(l));

                _L = l;
            }
        }

        public Hsl(double h, double s, double l)
        {
            if(h < 0 || h > 1)
                throw new ArgumentOutOfRangeException(nameof(h));
            if(s < 0 || s > 1)
                throw new ArgumentOutOfRangeException(nameof(s));
            if(l < 0 || l > 1)
                throw new ArgumentOutOfRangeException(nameof(l));

            _H = h;
            _S = s;
            _L = l;
        }
    }


    // http://web.archive.org/web/20110425154034/http://www.bobpowell.net/RGBHSB.htm
    public class HslConverter
    {
        public static Hsl ColorToHsl(Color color) => new Hsl(color.GetHue() / 360.0, color.GetSaturation(), color.GetBrightness());
        public static Color HslToColor(Hsl hsl)
        {
            double r = 0, g = 0, b = 0;
            double temp1, temp2;

            if (hsl.L == 0)
            {
                r = g = b = 0;
            }
            else if (hsl.S == 0)
            {
                r = g = b = hsl.L;
            }
            else
            {
                temp2 = ((hsl.L <= 0.5) ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S - (hsl.L * hsl.S));
                temp1 = 2.0 * hsl.L - temp2;

                double[] t3 = new double[] { hsl.H + 1.0 / 3.0, hsl.H, hsl.H - 1.0 / 3.0 };
                double[] clr = new double[] { 0, 0, 0 };

                for (int i = 0; i < 3; i++)
                {
                    if (t3[i] < 0)
                        t3[i] += 1.0;
                    if (t3[i] > 1)
                        t3[i] -= 1.0;

                    if (6.0 * t3[i] < 1.0)
                        clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
                    else if (2.0 * t3[i] < 1.0)
                        clr[i] = temp2;
                    else if (3.0 * t3[i] < 2.0)
                        clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
                    else
                        clr[i] = temp1;
                }

                r = clr[0];
                g = clr[1];
                b = clr[2];
            }

            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        public static Color SetHsl(Color color, double hue, double saturation, double brightness)
        {
            if (hue < 0 || hue > 1)
                throw new ArgumentOutOfRangeException(nameof(hue));
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException(nameof(brightness));

            Hsl hsl = ColorToHsl(color);
            hsl.H = hue;
            hsl.S = saturation;
            hsl.L = brightness;

            return HslToColor(hsl);
        }
        public static Color ModifyHsl(Color color, double hue, double saturation, double brightness)
        {
            if (hue < 0 || hue > 1)
                throw new ArgumentOutOfRangeException(nameof(hue));
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException(nameof(brightness));

            Hsl hsl = ColorToHsl(color);
            hsl.H *= hue;
            hsl.S *= saturation;
            hsl.L *= brightness;

            return HslToColor(hsl);
        }


        public static Color SetBrightness(Color color, double brightness)
        {
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException(nameof(brightness));

            Hsl hsl = ColorToHsl(color);
            hsl.L = brightness;

            return HslToColor(hsl);
        }
        public static Color ModifyBrightness(Color color, double brightness)
        {
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException(nameof(brightness));

            Hsl hsl = ColorToHsl(color);
            hsl.L *= brightness;

            return HslToColor(hsl);
        }

        public static Color SetSaturation(Color color, double saturation)
        {
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));

            Hsl hsl = ColorToHsl(color);
            hsl.S = saturation;

            return HslToColor(hsl);
        }
        public static Color ModifySaturation(Color color, double saturation)
        {
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));

            Hsl hsl = ColorToHsl(color);
            hsl.S *= saturation;

            return HslToColor(hsl);
        }

        public static Color SetHue(Color color, double hue)
        {
            if (hue < 0 || hue > 1)
                throw new ArgumentOutOfRangeException(nameof(hue));

            Hsl hsl = ColorToHsl(color);
            hsl.H = hue;

            return HslToColor(hsl);
        }
        public static Color ModifyHue(Color color, double hue)
        {
            if (hue < 0 || hue > 1)
                throw new ArgumentOutOfRangeException(nameof(hue));

            Hsl hsl = ColorToHsl(color);
            hsl.H *= hue;

            return HslToColor(hsl);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:플랫폼 호환성 유효성 검사", Justification = "<보류 중>")]
        public static Bitmap ColorChangeByHsl(Bitmap srcBit, float hue, float saturation, float brightness)
        {
            if (hue < 0 || hue > 1)
                throw new ArgumentOutOfRangeException(nameof(hue));
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException(nameof(brightness));

            BitmapData srcData = srcBit.LockBits(new Rectangle(0, 0, srcBit.Width, srcBit.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            Bitmap dstBit = new Bitmap(srcBit.Width, srcBit.Height);
            BitmapData dstData = dstBit.LockBits(new Rectangle(0, 0, dstBit.Width, dstBit.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //int bytesPerPixel = Bitmap.GetPixelFormatSize(srcBit.PixelFormat) / 8;

            HslProcess(srcData, dstData, hue, saturation, brightness);

            srcBit.UnlockBits(srcData);
            dstBit.UnlockBits(dstData);

            return dstBit;
        }

        /// <summary>
        /// Only 4Byte: bytesPerPixel
        /// </summary>
        /// <param name="srcBitData">SourceData</param>
        /// <param name="dstBitData">DestinationData</param>
        /// <param name="h">Hue</param>
        /// <param name="s">Satuation</param>
        /// <param name="l">Light</param>
        /// <param name="bytesPerPixel"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:플랫폼 호환성 유효성 검사", Justification = "<보류 중>")]
        internal static void HslProcess(BitmapData srcBitData, BitmapData dstBitData, float h, float s, float l)
        {
            unsafe
            {
                byte* srcData = (byte*)srcBitData.Scan0;
                byte* dstData = (byte*)dstBitData.Scan0;

                int pMaxIndex = srcBitData.Stride * srcBitData.Height / 4;
                Parallel.For(0, pMaxIndex, i =>
                {
                    int pixelIndex = i * 4;
                    byte a = srcData[pixelIndex + 3];

                    if (a > 0)
                    {
                        Color pixelColor = Color.FromArgb(srcData[pixelIndex + 2], srcData[pixelIndex + 1], srcData[pixelIndex]);
                        Color convertedColor = HslConverter.ModifyHsl(pixelColor, h, s, l);

                        dstData[pixelIndex] = convertedColor.B;
                        dstData[pixelIndex + 1] = convertedColor.G;
                        dstData[pixelIndex + 2] = convertedColor.R;
                        dstData[pixelIndex + 3] = a;
                    }
                });
            }
        }
    }
}