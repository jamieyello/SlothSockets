using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    // 0 - IsNull
    // 0 - IsArray
    // 000000000000000000000000000000000000000000000000000000000000000000000000 - Length (only included if IsArray && !IsNull)
    internal struct ObjectSerialationFlags
    {
        public bool IsNull;
        public bool IsArray;
        public long ArrayLength;
    }
}
