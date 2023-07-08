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

        void CheckCanReadAmount(byte length)
        {
            if (Position <= Bits.TotalLength + length) throw new ArgumentOutOfRangeException($"End of {nameof(BitBuilder)} reached.");
        }

        (byte XPos, int YPos) GetCoordinates() => ((byte)(Position % 64), (int)Position / 64);

        public bool ReadBool()
        {
            CheckCanReadAmount(1);
            var (x_pos, y_pos) = GetCoordinates();
            Position++;
            return ((Bits.Bits[y_pos] >> x_pos) & 1) > 0;
        }

        public byte ReadByte() => (byte)Read(8);

        public ulong Read(byte length)
        {
            CheckCanReadAmount(length);
            var (x_pos, y_pos) = GetCoordinates();
            Position += length;

            var remainder = 64 - length;

            if (x_pos <= remainder) return Bits.Bits[y_pos] >> remainder;
            else if (x_pos == remainder) return Bits.Bits[y_pos];
            else {
                var result = Bits.Bits[y_pos] << x_pos - remainder;
                result |= Bits.Bits[y_pos + 1] >> 64 - (x_pos + length) % 64;
                return result;
            }
        }

    }
}
