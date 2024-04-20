using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace sip.Utils;

public static class ObjectExtensions
{
    // public static T DeepClone<T>(this object obj)
    // {
    //     return (T)JsonSerializer.Deserialize(JsonSerializer.Serialize(obj), obj.GetType());
    // }

    /// <summary>
    /// Attempts to copy property of the object to the object given as argument.
    /// </summary>
    /// <param name="obj">Copy source</param>
    /// <param name="propertyName"></param>
    /// <param name="target">Copy target</param>
    public static bool CopyPropertyTo(this object obj, string propertyName, object target)
    {
        var sourceType = obj.GetType();
        var targetType = target.GetType();
        var sourceProp = sourceType.GetProperty(propertyName);
        if (sourceProp is null) return false; // Property not found on source object
        var targetProp = targetType.GetProperty(propertyName);
        if (targetProp is null) return false; // Property not found on target object
        if (targetProp.PropertyType != sourceProp.PropertyType) return false; // Property types do not match
            
        targetProp.SetValue(target, sourceProp.GetValue(obj));
        return true;
    }

    public static bool IsComplex(Type type) =>
        !( type.IsPrimitive || type.IsValueType || type.IsEnum || type == typeof(string) );

    public static bool IsComplex(PropertyInfo property) => IsComplex(property.PropertyType);

    public static object CopyObjectTo(this object objSource, object? target = null)
    {
        var sourceType = objSource.GetType();
        if (!IsComplex(sourceType))
        {
            throw new NotSupportedException($"Source object has to be a complex type, type {sourceType.FullName} was given.");
        }
            
        var propertyFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            
        var targetType = (target is null) ? sourceType : target.GetType();
        target ??= Activator.CreateInstance(targetType)!;
 
        PropertyInfo[] targetProperties = targetType.GetProperties(propertyFlags);
            
        // Source object might be a collection, in that case, do not enumerate properties. 
        // Instead, enumerate collection itself and copy the items.
        // But for now, this is not implemented, skip, TODO
        if (sourceType.GetInterface(nameof(System.Collections.IEnumerable)) is not null)
            return target; // TODO

        foreach (PropertyInfo tProperty in targetProperties)
        {
            if (!tProperty.CanWrite)
                continue;

            // Lookup the property on the source object
            var sProperty = sourceType.GetProperty(tProperty.Name);
            if (sProperty is null) continue; // No such property on source
            if (!tProperty.PropertyType.IsAssignableFrom(sProperty.PropertyType))
                continue; // Properties are not compatible

            if (IsComplex(tProperty))
            {
                // The property is complex type, we need to recurse, if not null
                object? propVal = sProperty.GetValue(objSource);
                tProperty.SetValue(target, propVal?.CopyObjectTo());
            }
            else
            {
                // Property is not of complex type, just assign
                tProperty.SetValue(target, sProperty.GetValue(objSource, null), null);
            }
        }

        return target;
    }

    public static T ShallowCopy<T>(this T source, T? target = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // Create a new instance of the object if target is not provided
        T clone = target ?? (T) Activator.CreateInstance(source.GetType())!;

        // Get all fields of the object, including private and non-public ones
        FieldInfo[] fields = source.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Copy the values of each field from the source object to the clone
        foreach (var field in fields)
        {
            object? value = field.GetValue(source);
            field.SetValue(clone, value);
        }

        return clone;
    }

    public static bool IsStringOrPrimitive(this object obj)
    {
        return obj.GetType().IsPrimitive || obj is string;
    }

    public static dynamic? DynamicInvoke(this object obj, string methodName, Type returnType, params object[] arguments)
    {
        var type = obj.GetType();
        var argtypes = arguments.Select(a => a.GetType()).ToArray();
        var meth = type.GetMethod(methodName, argtypes);
        

        if (meth is null || meth.ReturnType != returnType)
        {
            // Lets try to lookup in private interface implementations
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            meth = methods.FirstOrDefault(m =>
                m.ReturnType == returnType && m.Name.EndsWith(methodName) &&
                argtypes.SequenceEqual(m.GetParameters().Select(p => p.ParameterType)));
        }

        if (meth is null) throw new InvalidOperationException("Method not found");

        if (meth.ReturnType != returnType)
            throw new InvalidOperationException(
                $"DynamicInvoke: Return types do not match {returnType.Name}!={meth.ReturnType.Name}");


        return meth.Invoke(obj, arguments);
    }

    public static T? PickValue<T>(this IDictionary from, string key, T? def = default)
    {
        if (from.Contains(key))
            return (T?)from[key];

        return def;
    }
    
    public static T? SafePickValue<T>(this object from, string key)
    {
        if (from is IDictionary dicFrom)
        {
            var val = dicFrom[key];
            if (val is T vtyped) return vtyped;
            // throw new InvalidOperationException($"Value is not of type {typeof(T).Name}");
        }
        else
        {
            // No dictionary? Probably object property then
            var prop = from.GetType().GetProperty(key); 
            var val = prop?.GetValue(from);
            //Logger.LogDebug("Picking value {} {} on object: {} {} at property {} {}", val, val?.GetType().Name, from, from.GetType().Name, key, prop);
            if (val is T vtyped) return vtyped;
        }
        
        return default;
    }
    
    public static bool ShouldSetDefault(object? def, object? current)
    {
        if (def is null) return false;
        var type = def.GetType();
        object? defaultt = null; 
        if (type.IsValueType)
        {
            defaultt = Activator.CreateInstance(type);
        }

        if (def is string defstr && current is string currentstr)
        {
            // For strings, we assume that "" is also empty value, same as null
            // default equality comparison is therefore not enough
            return !string.IsNullOrEmpty(defstr) && string.IsNullOrEmpty(currentstr);
        }
        
        // Not string
        if (!Equals(def, defaultt) && Equals(current, defaultt))
        {
            return true;
        }

        return false;
    }

    public static bool IsDefault(object? obj)
    {
        if (obj is null) return true;
        var type = obj.GetType();
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return obj.Equals(defaultValue);
        }

        return false; // Not null, not value type => not default
    }
    


    public static Dictionary<string, object?> ToDictionary(this JsonElement node)
    {
        var result = node.ToObject();
        if (result is not Dictionary<string, object?> res)
            throw new ArgumentException($"Given JsonNode (of kind: {node.ValueKind}) must be of object kind in order to map it into dictionary");
        
        return res;
    }
    
    public static object? ToObject(this JsonElement node)
    {
        switch (node.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var property in node.EnumerateObject())
                {
                    dict[property.Name] = property.Value.ToObject();
                }

                return dict;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in node.EnumerateArray())
                {
                    list.Add(item.ToObject());
                }

                return list;

            case JsonValueKind.String:
                return node.GetString();

            case JsonValueKind.Number:
                if (node.TryGetInt32(out int intValue))
                    return intValue;
                if (node.TryGetInt64(out long longValue))
                    return longValue;
                if (node.TryGetDouble(out double doubleValue))
                    return doubleValue;
                if (node.TryGetDecimal(out decimal decimalValue))
                    return decimalValue;
                return node.GetRawText();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return node.GetRawText();
        }
    }

}