using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    /// <summary> Reads data from a <see cref="BitBuilder"/>. Created from <see cref="BitBuilder.GetReader"/>. </summary>
    /// <remarks> Starts from the beginning. Each reader created keeps its own independent position. </remarks>
    public class BitBuilderReader
    {
        public BitBuilder Bits { get; private set; }
        
        public long Position { get; set; }

        internal BitBuilderReader(BitBuilder bits)
        {
            Bits = bits;
        }

        void CheckCanReadAmount(long length)
        {
            if (Position + length > Bits.TotalLength) throw new Exception($"End of {nameof(BitBuilder)} reached.");
        }

        (byte XPos, int YPos) GetCoordinates() => ((byte)(Position % 64), (int)Position / 64);

        public bool ReadBool()
        {
            CheckCanReadAmount(1);
            var (x_pos, y_pos) = GetCoordinates();
            Position++;
            return ((Bits.Bits[y_pos] >> x_pos) & 1) > 0;
        }

        public sbyte ReadSByte() => (sbyte)Read(8);
        public byte ReadByte() => (byte)Read(8);

        public ushort ReadUShort() => (ushort)Read(16);
        public short ReadShort() => (short)Read(16);
        public char ReadChar() => (char)Read(16);

        public uint ReadUInt() => (uint)Read(32);
        public int ReadInt() => (int)Read(32);

        public ulong ReadULong() => (ulong)Read(64);
        public long ReadLong() => (long)Read(64);

        public DateTime ReadDateTime() => new((long)Read(64), (DateTimeKind)Read(32));
        public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks((long)Read(64));

        public decimal ReadDecimal() => new(ReadArray<int>(32, 4));

        public string ReadString() {
            var length = ReadInt();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++) sb.Append(ReadChar());
            return sb.ToString();
        }

        public sbyte[] ReadSBytes(long count) => ReadArray<sbyte>(8, count);
        public byte[] ReadBytes(long count) => ReadArray<byte>(8, count);

        public ushort[] ReadUShorts(long count) => ReadArray<ushort>(16, count);
        public short[] ReadShorts(long count) => ReadArray<short>(16, count);
        public char[] ReadChars(long count) => ReadArray<char>(16, count);

        public uint[] ReadUInts(long count) => ReadArray<uint>(32, count);
        public int[] ReadInts(long count) => ReadArray<int>(32, count);

        public ulong[] ReadULongs(long count) => ReadArray<ulong>(64, count);
        public long[] ReadLongs(long count) => ReadArray<long>(64, count);

        public string[] ReadStrings(long count) {
            var result = new string[count];
            for (long i = 0; i < count; i++) result[i] = ReadString();
            return result;
        }

        T[] ReadArray<T>(byte length, long count)
        {
            CheckCanReadAmount(length * count);
            var result = new T[count];

            // Todo: speedup here with a more complex re-implementation --
            for (long i = 0; i < count; i++) result[i] = (T)(object)Read(length);
            // --

            return result;
        }

        ulong Read(byte length)
        {
            CheckCanReadAmount(length);
            var (x_pos, y_pos) = GetCoordinates();
            Position += length;

            var remainder = 64 - length;

            if (x_pos <= remainder) return Bits.Bits[y_pos] >> remainder - x_pos;
            else if (x_pos == remainder) return Bits.Bits[y_pos];
            else {
                var result = Bits.Bits[y_pos] << x_pos - remainder;
                result |= Bits.Bits[y_pos + 1] >> 64 - (x_pos + length) % 64;
                return result;
            }
        }
    }
}
