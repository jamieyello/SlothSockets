using Newtonsoft.Json.Linq;
using SlothSockets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Tests
{
    [TestClass]
    public class BitBuilderIntegrity
    {
        [TestMethod]
        public void IntegrityTestRand()
        {
            var rand = new Random(2);
            var bb = new BitBuilder();

            bb.Append(true);
            Console.WriteLine("Expected;");
            for (int i = 0; i < 10; i++) {
                var d = (byte)rand.Next();
                bb.Append(d);
                Console.WriteLine(d);
            }
            Console.WriteLine();

            Console.WriteLine("Results;");
            rand = new(2);
            var reader = bb.GetReader();
            reader.ReadBool();
            for (int i = 0; i < 10; i++) { 
                var d = reader.ReadByte();
                Console.WriteLine(d);
                Assert.AreEqual((byte)rand.Next(), d);
            }
        }

        [TestMethod]
        public void IntegrityTestString()
        {
            var message = "Wowowowow.";

            var bb = new BitBuilder();
            bb.Append(true);
            bb.Append(message);

            var reader = bb.GetReader();

            reader.ReadBool();
            var s = reader.ReadString();
            Assert.AreEqual(message, s);
        }




    }
}
