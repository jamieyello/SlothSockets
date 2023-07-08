using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
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

        public void WriteDebug() {
            foreach (var ul in Bits) Console.WriteLine(Convert.ToString((long)ul, 2).PadLeft(64, '0'));
        }

        public BitBuilderReader GetReader() => new(this);

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

        public void Append(string value) {
            Append(value.Length);
            for (int i = 0; i < value.Length; i++) Append(value[i]);
        }

        public void Append(bool[] value) {
            var span = new ReadOnlySpan<bool>(value);
            for (int i = 0; i < value.Length; i++) Append(span[i]);
        }

        public void Append(sbyte[] value) {
            var span = new ReadOnlySpan<sbyte>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 8); 
        }
        public void Append(byte[] value) {
            var span = new ReadOnlySpan<byte>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 8);
        }

        public void Append(ushort[] value) {
            var span = new ReadOnlySpan<ushort>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 16);
        }
        public void Append(short[] value) {
            var span = new ReadOnlySpan<short>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 16);
        }
        public void Append(char[] value) {
            var span = new ReadOnlySpan<char>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 16);
        }

        public void Append(uint[] value) {
            var span = new ReadOnlySpan<uint>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 32);
        }
        public void Append(int[] value) {
            var span = new ReadOnlySpan<int>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 32);
        }

        public void Append(ulong[] value) {
            var span = new ReadOnlySpan<ulong>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 64);
        }
        public void Append(long[] value) {
            var span = new ReadOnlySpan<long>(value);
            for (int i = 0; i < value.Length; i++) Append((ulong)span[i], 64);
        }
        public void Append(string[] value) {
            var span = new ReadOnlySpan<string>(value);
            for (int i = 0; i < value.Length; i++) Append(span[i]);
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
