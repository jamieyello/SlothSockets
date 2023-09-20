using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    /// <summary> A special object that is prepended in front of serialized classes and structs in <see cref="BitBuilderSerializer"/>. Used to help de-serialization. </summary>
    /// <remarks> See <see cref="BitBuilder.Append(ObjectSerialationFlags)"/> and <see cref="BitBuilderReader.ReadObjectSerializationFlags"/> to see exactly how this is serialized. All mentioned methods must be modified if this structure is. </remarks>
    internal struct ObjectSerialationFlags
    {
        public bool IsNull;
        public bool IsICollection;
        public long Length;
        public bool IsArray;
        public ushort ArrayDimensionCount;
        public long[] ArrayLengths;
    }
}
