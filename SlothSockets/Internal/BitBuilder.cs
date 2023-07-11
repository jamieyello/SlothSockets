using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    /// <summary> In this class we use a ulong[] instead of byte[] to store data to take up the full memory space. </summary>
    /// <remarks> This is 8 times more memory efficient than a byte[] at the cost of using more CPU power. </remarks>
    public class BitBuilder
    {
        internal List<ulong> Bits { get; set; } = new() { 0 };
        internal byte XPos { get; set; } = 0;
        /// <summary> Total size in bits. </summary>
        public long TotalLength => (Bits.Count - 1) * 64 + XPos;

        static readonly HashSet<Type> base_types = new() {
            typeof(bool),
            typeof(sbyte),
            typeof(byte),
            typeof(ushort),
            typeof(short),
            typeof(char),
            typeof(decimal),
            typeof(uint),
            typeof(int),
            typeof(ulong),
            typeof(long),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(string),
            typeof(ObjectSerialationFlags),
        };

        internal static bool IsBaseSupportedType(Type type) => base_types.Contains(type);

        public void WriteDebug() {
            foreach (var ul in Bits) Console.WriteLine(Convert.ToString((long)ul, 2).PadLeft(64, '0'));
        }

        public BitBuilderReader GetReader() => new(this);

        static MethodInfo[] GetPublicAppendMethods()
        {
            List<MethodInfo> result = new();

            var append_methods = typeof(BitBuilder).GetMethods(BindingFlags.Public)
                .Where(method => method.Name == "Append");

            foreach (var append_method in append_methods)
            {
                var parameters = append_method.GetParameters();
                if (parameters.Length != 1) continue;
            }

            throw new NotImplementedException();
        }
        static internal Dictionary<Type, FastMethodInfo> cache_AppendPrimative = new();
        internal void AppendBaseTypeObject(object obj)
        {
            if (cache_AppendPrimative.TryGetValue(obj.GetType(), out var method)) method.Invoke(this, obj);
            MethodInfo target = GetPublicAppendMethods()
                .Where(method => method.GetParameters()[0].ParameterType == obj.GetType())
                .FirstOrDefault() ?? throw new Exception("Object is not a base supported type.");

            var fast_method = new FastMethodInfo(target);
            cache_AppendPrimative.Add(obj.GetType(), fast_method);
            fast_method.Invoke(this, obj);
        }

        public void Append(bool value)  {
            if (XPos == 64) {
                Bits.Add(0);
                XPos = 0;
            }

            Bits[^1] |= (value ? (ulong)1 : 0) << 63 - XPos++;
        }

        public void Append(sbyte value) => Append((ulong)value, 8);
        public void Append(byte value) => Append((ulong)value, 8);

        public void Append(ushort value) => Append((ulong)value, 16);
        public void Append(short value) => Append((ulong)value, 16);
        public void Append(char value) => Append((ulong)value, 16);

        public void Append(uint value) => Append((ulong)value, 32);
        public void Append(int value) => Append((ulong)value, 32);

        public void Append(ulong value) => Append((ulong)value, 64);
        public void Append(long value) => Append((ulong)value, 64);

        public void Append(DateTime value) {
            Append((ulong)value.Ticks, 64);
            Append((ulong)value.Kind, 32);
        }
        public void Append(TimeSpan value) => Append((ulong)value.Ticks, 64);

        public void Append(decimal value) => Append(decimal.GetBits(value));

        public void Append(string value) {
            Append(value.Length);
            for (int i = 0; i < value.Length; i++) Append(value[i]);
        }

        internal void Append(ObjectSerialationFlags object_flags)
        {
            Append(object_flags.IsNull);
        }

        internal void Append(object obj, SerializeMode mode = SerializeMode.Properties) => BitBuilderSerializer.Serialize(obj, this, mode);

        public void Append(IList<bool> value) {
            for (int i = 0; i < value.Count; i++) Append(value[i]);
        }

        public void Append(IList<sbyte> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 8); 
        }
        public void Append(IList<byte> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 8);
        }

        public void Append(IList<ushort> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
        }
        public void Append(IList<short> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
        }
        public void Append(IList<char> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
        }

        public void Append(IList<uint> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 32);
        }
        public void Append(IList<int> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 32);
        }

        public void Append(IList<ulong> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 64);
        }
        public void Append(IList<long> value) {
            for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 64);
        }
        public void Append(IList<string> value) {
            for (int i = 0; i < value.Count; i++) Append(value[i]);
        }

        void Append(ulong value, byte length) {
            var remainder = 64 - length;

            if (XPos < remainder) {
                Bits[^1] |= value << remainder - XPos;
                XPos += length;
            }
            else if (XPos == remainder) {
                Bits[^1] |= value;
                Bits.Add(0);
                XPos = 0;
            }
            else {
                Bits[^1] |= value >> XPos - remainder;
                XPos += length;
                XPos %= 64;
                Bits.Add(0);
                Bits[^1] |= value << 64 - XPos;
            }
        }
    }
}
