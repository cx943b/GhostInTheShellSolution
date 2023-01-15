using GhostInTheShell.Modules.InfraStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class HslTest
    {
        const int LoopCount = 1000000;

        [TestMethod]
        public void ConvertTest()
        {
            Random rand = new Random();

            Color color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            Hsl hsl = HslConverter.ColorToHsl(color);
            Color convertedColor = HslConverter.HslToColor(hsl);

            Assert.AreEqual(color, convertedColor);
        }

        [TestMethod]
        public void RgbToHslLoopTest()
        {
            Stopwatch watch = new Stopwatch();
            Color color = Color.FromArgb(222, 111, 123);

            watch.Start();
            for(int i = 0; i < LoopCount; ++i)
            {
                HslConverter.ColorToHsl(color);
            }
            watch.Stop();

            Debug.WriteLine($"RgbToHslLoopTest: {watch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public void HslToRgbLoopTest()
        {
            Stopwatch watch = new Stopwatch();
            Hsl hsl = new Hsl(0.5f, 0.1f, 0.4f);

            watch.Start();
            for (int i = 0; i < LoopCount; ++i)
            {
                HslConverter.HslToColor(hsl);
            }
            watch.Stop();

            Debug.WriteLine($"HslToRgbLoopTest: {watch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public void ImageConvertTest()
        {
            Bitmap srcBit = (Bitmap)Bitmap.FromFile("髪f_03.png");
            Stopwatch watch = new Stopwatch();

            watch.Start();
            Bitmap dstBit = HslConverter.ColorChangeByHsl(srcBit, 0.5f, 0f, 0.5f);
            watch.Stop();

            Debug.WriteLine($"ImageConvertTest: {watch.ElapsedMilliseconds}ms");

            dstBit.Save("ConvertTested.png", ImageFormat.Png);

            dstBit.Dispose();
            srcBit.Dispose();
        }
    }
}
