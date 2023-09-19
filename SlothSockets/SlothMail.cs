using SlothSockets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SlothSockets
{
    public class SlothMail
    {
        BitBuilder data;
        SerializeMode mode;

        public SlothMail()
        {
            
        }

        public SlothMail(object obj, SerializeMode mode = SerializeMode.Fields)
        {
            this.mode = mode;
        }
    }
}
