using SlothSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary> Specifies how a class should be serialized by SlothSockets. </summary>
[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
public class SlothSerializeAttribute : System.Attribute
{
    public readonly SerializeMode mode;

    public SlothSerializeAttribute(SerializeMode mode = SerializeMode.Fields)
    {
        this.mode = mode;
    }
}