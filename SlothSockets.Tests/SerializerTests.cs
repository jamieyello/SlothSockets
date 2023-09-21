using SlothSockets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Tests
{
    [TestClass]
    public class SerializerTests
    {
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
            public ulong test_value;
            public ulong[] test_array;
            public ulong[,] test_array2;
            public string test_string;

            public bool Matches(TestClass2 t) =>
                test_value == t.test_value &&
                test_array.SequenceEqual(t.test_array) &&
                test_string == t.test_string;
        }

        class TestClass3
        {
            public ulong test_value1;
            public ulong[] test_values;
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
                //test_array = new ulong[] { 1, 2, 3 },
                test_array2 = new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } },
                //test_string = "wowowow",
                //test_value = 3
            };
            bb.Append(original, SerializeMode.Fields);
            bb.WriteDebug();

            var read = bb.GetReader().Read<TestClass2>()
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest3()
        {
            var bb = new BitBuilder();
            var value = ulong.MaxValue / 2;
            var original = new TestClass3()
            {
                test_value1 = value,
                test_values = new[] { value, value, value,}
            };
            bb.Append(original, SerializeMode.Fields);
            bb.WriteDebug();

            var read = bb.GetReader().Read<TestClass3>()
                ?? throw new Exception("Read null.");
            //Assert.IsTrue(original.Matches(read));
        }
    }
}
