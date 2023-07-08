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
        byte[]? data { get; set; }

        public SlothMail()
        {
            
        }

        public SlothMail(object obj, SerializeMode mode = SerializeMode.Fields)
        {
            
        }
    }
}
