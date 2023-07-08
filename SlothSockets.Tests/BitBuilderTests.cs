using SlothSockets.Internal;
using System.Diagnostics;

namespace SlothSockets.Tests
{
    [TestClass]
    public class BitBuilderTests
    {
        [TestMethod]
        public void BitBench1()
        {
            var bb = new BitBuilder();
            for (int i = 0; i < 100000000; i++) bb.Append(i);
        }

        [TestMethod]
        public void BitBench2()
        {
            var bb = new LowMemList<int>();
            for (int i = 0; i < 100000000; i++) bb.Add(i);
        }
    }
}