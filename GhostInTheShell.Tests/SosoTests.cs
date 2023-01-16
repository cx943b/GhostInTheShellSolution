using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class SosoTests
    {
        [TestMethod]
        public void StringBoolTest()
        {
            const int LoopCount = 10000000;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            compareString(LoopCount);
            watch.Stop();

            Debug.WriteLine($"CompareString: {watch.ElapsedMilliseconds}ms");

            watch.Restart();
            compareConvert(LoopCount);
            watch.Stop();

            Debug.WriteLine($"CompareConvert: {watch.ElapsedMilliseconds}ms");

        }

        private void compareString(int loopCount)
        {
            const string bTrue = "true";

            for (int i = 0; i < loopCount; ++i)
            {
                bool isTrue = String.Compare(bTrue, "true", true) == 0;
            }
        }
        private void compareConvert(int loopCount)
        {
            const string bTrue = "true";

            for (int i = 0; i < loopCount; ++i)
            {
                bool istrue = Boolean.Parse(bTrue);
            }
        }
    }
}
