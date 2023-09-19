using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    /// <summary> Reads data from a <see cref="BitBuilder"/>. Created from <see cref="BitBuilder.GetReader"/>. </summary>
    /// <remarks> Starts from the beginning. Each reader created keeps its own independent position. 
    /// TODO: Too low level for public use.
    /// </remarks>
    public class BitBuilderReader
    {
        public BitBuilder Bits { get; private set; }
        
        public long Position { get; set; }

        readonly Dictionary<Type, Func<long, object>> read_methods;

        internal BitBuilderReader(BitBuilder bits)
        {
            Bits = bits;

            read_methods = new() {
                { typeof(bool), (c) => ReadBool() },
                { typeof(sbyte), (c) => ReadSByte() },
                { typeof(byte), (c) => ReadByte() },
                { typeof(ushort), (c) => ReadUShort() },
                { typeof(short), (c) => ReadShort() },
                { typeof(char), (c) => ReadChar() },
                { typeof(decimal), (c) => ReadDecimal() },
                { typeof(uint), (c) => ReadUInt() },
                { typeof(int), (c) => ReadInt() },
                { typeof(ulong), (c) => ReadULong() },
                { typeof(long), (c) => ReadLong() },
                { typeof(DateTime), (c) => ReadDateTime() },
                { typeof(TimeSpan), (c) => ReadTimeSpan() },
                { typeof(string), (c) => ReadString() },

                { typeof(bool[]), ReadBools },
                { typeof(sbyte[]), ReadSBytes },
                { typeof(byte[]), ReadBytes },
                { typeof(ushort[]), ReadUShorts },
                { typeof(short[]), ReadShorts },
                { typeof(char[]), ReadChars },
                { typeof(decimal[]), ReadDecimals },
                { typeof(uint[]), ReadUInts },
                { typeof(int[]), ReadInts },
                { typeof(ulong[]), ReadULongs },
                { typeof(long[]), ReadLongs },
                { typeof(DateTime[]), ReadDateTimes },
                { typeof(TimeSpan[]), ReadTimeSpans },
                { typeof(string[]), ReadStrings },

                // unneeded
                //{ typeof(ObjectSerialationFlags), () => ReadObjectSerializationFlags() },
            };
        }

        // Debatable whether this should be here or not. (High level method in low level class)
        public T? Read<T>(SerializeMode mode = SerializeMode.Fields)
        {
            return (T?)BitBuilderSerializer.DeSerialize(typeof(T), this, mode);
        }

        internal object Read(Type type, long? array_length = 0)
        {
            if (read_methods.TryGetValue(type, out var method))
            {
                return method.Invoke(array_length ?? 0);
            }
            else throw new NotImplementedException();
        }

        internal bool IsSupportedType(Type type) =>
            read_methods.ContainsKey(type);

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

        public bool[] ReadBools(long count) => ReadArray<bool>(1, count);

        public sbyte[] ReadSBytes(long count) => ReadArray<sbyte>(8, count);
        public byte[] ReadBytes(long count) => ReadArray<byte>(8, count);

        public ushort[] ReadUShorts(long count) => ReadArray<ushort>(16, count);
        public short[] ReadShorts(long count) => ReadArray<short>(16, count);
        public char[] ReadChars(long count) => ReadArray<char>(16, count);

        public uint[] ReadUInts(long count) => ReadArray<uint>(32, count);
        public int[] ReadInts(long count) => ReadArray<int>(32, count);

        public ulong[] ReadULongs(long count) => ReadArray<ulong>(64, count);
        public long[] ReadLongs(long count) => ReadArray<long>(64, count);

        public DateTime[] ReadDateTimes(long count)
        {
            var result = new DateTime[count];
            for (long i = 0; i < count; i++) result[i] = ReadDateTime();
            return result;
        }

        public TimeSpan[] ReadTimeSpans(long count)
        {
            var result = new TimeSpan[count];
            for (long i = 0; i < count; i++) result[i] = ReadTimeSpan();
            return result;
        }

        public decimal[] ReadDecimals(long count)
        {
            var result = new decimal[count];
            for (long i = 0; i < count; i++) result[i] = ReadDecimal();
            return result;
        }

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

        internal ObjectSerialationFlags ReadObjectSerializationFlags()
        {
            var flags = new ObjectSerialationFlags();
            flags.IsNull = ReadBool();
            flags.IsArray = ReadBool();
            if (flags.IsArray && !flags.IsNull) flags.ArrayLength = ReadLong();
            return flags;
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
