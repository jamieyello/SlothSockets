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

        // Tested;
        // class, struct
        // all/most common base value types
        //
        // Tbt;
        // Arrays, null values, T? types, properties, attributes

        #region Test Classes
        // we don't actually care if this breaks formatting
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        struct TestClass1
        {
            public ulong test1;
            public ulong test2;
            public ulong test3;
            public ulong test4;
            public string test_string;
        }

        class TestClass2
        {
            //public ulong test_value;
            public ulong[] test_array;
            //public string test_string;

            public bool Matches(TestClass2 t) =>
                //test_value == t.test_value &&
                test_array.SequenceEqual(t.test_array) /*&&*/
                /*test_string == t.test_string*/;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        #endregion

        /// <summary>
        /// Simple serialization test.
        /// </summary>
        [TestMethod]
        public void SerializeTest1()
        {
            var bb = new BitBuilder();
            var original = new TestClass1()
            {
                test1 = 1,
                test2 = 2,
                test3 = 3,
                test4 = 4,
                test_string = null
            };
            bb.Append(original, SerializeMode.Fields);
            bb.WriteDebug();

            var read = bb.GetReader().Read<TestClass1>();
            Assert.AreEqual(original, read);
        }

        [TestMethod]
        public void SerializeTest2()
        {
            var bb = new BitBuilder();
            var original = new TestClass2()
            {
                test_array = new ulong[] { 1, 2, 3 },
                //test_string = "wowowow",
                //test_value = 3
            };
            bb.Append(original, SerializeMode.Fields);
            bb.WriteDebug();

            var read = bb.GetReader().Read<TestClass2>() 
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }
    }
}
