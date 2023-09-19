using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    internal static class BitBuilderSerializer
    {
        // Reflection is slow unless you cache it.
        static readonly Dictionary<Type, SlothSerializeAttribute?> cache_GetSerializeAttribute = new();
        internal static SlothSerializeAttribute? GetSerializeAttribute(Type type) {
            if (cache_GetSerializeAttribute.TryGetValue(type, out var attribute)) return attribute;
            var result = type.GetCustomAttribute<SlothSerializeAttribute>();
            cache_GetSerializeAttribute.Add(type, result);
            return result;
        }

        /// <summary> A primitive type is a type with no underlying fields, like a bool int or string. </summary>
        internal static bool IsPrimitiveType(Type type) => 
            type.IsPrimitive || type.IsValueType || (type == typeof(string));

        static readonly Dictionary<Type, FieldInfo[]> cache_GetTargetFields = new();
        internal static FieldInfo[] GetTargetFields(Type type, SerializeMode mode) {
            if ((mode & SerializeMode.Fields) == 0) return Array.Empty<FieldInfo>();
            if (cache_GetTargetFields.TryGetValue(type, out var result)) return result;
            result = type.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(field => field.MetadataToken).ToArray();
            cache_GetTargetFields.Add(type, result);
            return result;
        }

        static readonly Dictionary<Type, object?> cache_GetDefault = new();
        public static object? GetDefault(Type type) {
            if (cache_GetDefault.TryGetValue(type, out var @default)) return @default;
            var result = type.IsValueType ? Activator.CreateInstance(type) : null;
            return result;
        }

        /// <summary> Serialize object to <see cref="BitBuilder"/>. </summary>
        /// <exception cref="NotImplementedException"> Thrown if a type in the object is not supported. </exception>
        internal static void Serialize(object? obj, BitBuilder builder, SerializeMode default_mode) {

            if (obj == null) {
                builder.Append(new ObjectSerialationFlags() { IsNull = true });
                return;
            }

            var type = obj.GetType();
            var attribute = GetSerializeAttribute(type);
            var mode = attribute?.Mode ?? default_mode;

            if (IsPrimitiveType(type)) {
                if (!BitBuilder.IsBaseSupportedType(type)) throw new NotImplementedException($"Type must by implemented in {nameof(BitBuilder)}.");
                builder.AppendBaseTypeObject(obj);
            }
            else {
                var t = GetTargetFields(type, default_mode);

                if (type.IsArray) { throw new NotImplementedException(); }

                builder.Append(new ObjectSerialationFlags() { IsNull = false, IsArray = type.IsArray });
                foreach (var field in GetTargetFields(type, default_mode)) {
                    // reflection slow?
                    Serialize(field.GetValue(obj), builder, default_mode);
                }
            }
        }

        internal static object? DeSerialize(Type type, BitBuilderReader reader, SerializeMode default_mode) {
            var attribute = GetSerializeAttribute(type);
            var mode = attribute?.Mode ?? default_mode;

            if (IsPrimitiveType(type)) {
                if (!reader.IsSupportedType(type)) throw new NotImplementedException($"Type must by implemented in {nameof(BitBuilder)}.");
                return reader.Read(type);
            }
            else
            {
                var flags = reader.ReadObjectSerializationFlags();
                if (flags.IsNull) return default;
                if (flags.IsArray) { throw new NotImplementedException(); }

                var obj = Activator.CreateInstance(type) 
                    ?? throw new Exception($"Failed to create instance of {type.FullName}");
                foreach (var field in GetTargetFields(type, default_mode))
                {
                    // reflection slow?
                    field.SetValue(obj, DeSerialize(field.FieldType, reader, default_mode));
                }
                return obj;
            }
        }
    }
}
