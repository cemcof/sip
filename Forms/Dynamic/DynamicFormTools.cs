using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.JsonPatch.Helpers;

namespace sip.Forms.Dynamic;

public record DynamicElementSetup(
    Type Type,
    object? Default,
    string? SpecifiedType = null,
    string? DisplayName = null,
    IList? Selection = null,
    string? Tip = null,
    bool IsReadonly = false,
    bool IsRequired = false,
    string? Unit = null,
    string? Scope = null,
    string? Flex = null,
    int Order = -1,
    int Min = int.MinValue,
    int Max = int.MaxValue	
)
{
    public static DynamicElementSetup FromObject(object? setup)
    {
        (Type, bool isSupported) GetTypeInfo(object? value)
        {
            if (value is null || value is string) return (typeof(string), true);
            // Handle lists
            if (value is IList list)
            {
                if (list.Count == 0) return (typeof(List<string>), true);
                var first = list[0];
                var (type, supported) = GetTypeInfo(first);
                if (!supported) return (typeof(string), false);
                return (typeof(List<>).MakeGenericType(type), true);
            }

            return (value.GetType(), false);
        }

        if (setup is string st && DynamicFormTools.TryParseSetupFromString(st, out var dynSetup))
            return dynSetup!;
        
        var tinfo = GetTypeInfo(setup);

        if (tinfo.isSupported)
        {
            return new DynamicElementSetup(Type: tinfo.Item1, Default: setup);
        }

        if (setup is DynamicElementSetup dyn)
            return dyn;
        
        if (setup is IDictionary dict && 
            (dict.Contains(nameof(Type)) || dict.Contains(nameof(Default)))) // TODO - more robust
        {
            // Type? type = null;
            // if (dict.Contains(nameof(Type)))
            //     type = DynamicFormTools.StringToValType(dict[nameof(Type)]!.ToString());
            //
            //
            // if (dict.Contains(nameof(Default)))
            // {
            //     var defType = 
            // }
            //     
            // var type = (dict.Contains(nameof(Type))) ?
            //     DynamicFormTools.StringToValType(dict[nameof(Type)]!.ToString()) :
            //     
            // var defaultVal = DynamicFormTools.ConvertToGivenType(type, dict[nameof(Default)].ToString());
            var bp = BindPoint.From(setup);
            
            return new DynamicElementSetup(
                Type: type,
                Default: defaultVal,
                SpecifiedType: bp.GetValueByKey<string>(nameof(DisplayName)),
                DisplayName: bp.GetValueByKey<string>(nameof(DisplayName)),
                Selection: bp.GetValueByKey<IList>(nameof(Selection)),
                Tip: bp.GetValueByKey<string>(nameof(Tip)),
                IsReadonly: bp.GetValueByKey<bool>(nameof(IsReadonly)),
                IsRequired: bp.GetValueByKey<bool>(nameof(IsRequired)),
                Unit: bp.GetValueByKey<string>(nameof(Unit)),
                Scope: bp.GetValueByKey<string>(nameof(Scope)),
                Flex: bp.GetValueByKey<string>(nameof(Flex)),
                Min: bp.GetValueByKey<int>(nameof(Min)),
                Max: bp.GetValueByKey<int>(nameof(Max))
            );
        }
        
        throw new NotSupportedException	($"Type {setup.GetType().Name} is not supported as dynamic form setup");
    }
    
    
}

public interface IBindPoint
{
    public object? GetValue();
    public void SetValue(object? value);

    public T? GetValueByKey<T>(string key, T? defaultValue = default);
    public T GetValue<T>(T defaultValue = default);
}

public class KBindPoint(ref string key)
{
    
}

public class BindPoint : IBindPoint
{
    /// <summary>
    /// Sets target object according to this BindPoint 
    /// </summary>
    /// <param name="target"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void EnsureTarget(ref object? target)
    {
        
    }

    public void SetTo(BindPoint target)
    {
        
    }

    public virtual object? GetValue()
    {
        throw new NotSupportedException();
    }

    public virtual void SetValue(object? value)
    {
        throw new NotSupportedException();
    }

    public virtual T GetValue<T>(T defaultValue = default)
    {
        throw new NotSupportedException();
    }

    public static IBindPoint From(object o, object key)
    {
        return o switch
        {
            IDictionary dict => new DictBindPoint(dict, key.ToString()!),
            IList list => new ListBindPoint(list, (int) key),
            _ => new ObjectBindPoint(o, key.ToString()!)
        };
    }

    public static T? Get<T>(object target, string key, T? defaulValue = default)
    {
        return From(target, key).GetValue(defaulValue);
    }
}


public class DictBindPoint(IDictionary dict, string key) : BindPoint
{
    public override object? GetValue()
    {
        return dict[key];
    }

    public override void SetValue(object? value)
    {
        dict[key] = value;
    }

    public override T GetValue<T>(T defaultValue = default)
    {
        if (dict[key] is T val) return val;
        return defaultValue;
    }
}

public class ListBindPoint(IList list, int index) : BindPoint
{
    public override object? GetValue()
    {
        return list[index];
    }

    public override void SetValue(object? value)
    {
        list[index] = value;
    }

    public override T GetValue<T>(T defaultValue = default)
    {
        if (list[index] is T val) return val;
        return defaultValue;
    }
}

public class ObjectBindPoint(object obj, string key) : BindPoint
{
    public override object? GetValue()
    {
        var prop = obj.GetType().GetProperty(key);
        if (prop is null) return null;
        return prop.GetValue(obj);
    }

    public override void SetValue(object? value)
    {
        var prop = obj.GetType().GetProperty(key);
        if (prop is null) return;
        prop.SetValue(obj, value);
    }

    public override T GetValue<T>(T defaultValue = default)
    {
        var prop = obj.GetType().GetProperty(key);
        if (prop is null) return defaultValue;
        if (prop.GetValue(obj) is T val) return val;
        return defaultValue;
    }
}

public class DynamicBindContext
{
    public DynamicBindContext(object source, object? target = null, string? listIdKey = null)
    {

    }
    
    

    public static bool ValueAplicator(BindPoint source, BindPoint target)
    {
        var sourceVal = source.GetValue();

        try
        {
            var dynObj = DynamicElementSetup.FromObject(sourceVal);
            target.SetValue(dynObj.Default);
            return false;
        }
        catch (NotSupportedException e)
        {
            source.SetTo(target);
            return true;
        }
    }

    public static bool ConvertToDynamicSetupAplicator(BindPoint source, BindPoint target)
    {
        var val = source.GetValue();
        try
        {
            var dynaSetup = DynamicElementSetup.FromObject(val);
            target.SetValue(dynaSetup);
            return false;
        }
        catch (NotSupportedException e)
        {
            source.SetTo(target);
            return true; // Continue recurse
        }
    }


public IEnumerable<BindPoint> ApplyValues(
        object source, 
        ref object? target,
        
        string? listIdKey = DynamicFormTools.ID_KEY,
        string? groupKey = DynamicFormTools.GROUP_NAME_KEY,
        string? groupDescriptionKey = DynamicFormTools.DESCRIPTION_KEY)
    {
        
        // Lets start by ensuring the target 
        var srcb = new BindPoint(source, null);
        srcb.EnsureTarget(ref target);
        // Now iterate the source object
        
        if (source is IDictionary dsource)
        {
            var k = dsource["fwef"];
            ApplyValues(source, ref dsource["fewf"]);
        } 
    }

    
    
    
    
}

public static class DynamicFormTools
{
    public const string ID_KEY = "ID";
    public const string TYPE_KEY = "TYPE";
    public const string DESCRIPTION_KEY = "DESCRIPTION";
    public const string NAME_KEY = "NAME";
    public const string GROUP_NAME_KEY = "GROUP_NAME";
    public const string 
    
    public static bool TryParseSetupFromString(string? strval, out DynamicElementSetup? result)
    {
        if (string.IsNullOrWhiteSpace(strval))
        {
            result = null;
            return false;
        }
        
        // Format from scipion
        if (strval.StartsWith("~") && strval.EndsWith("~"))
        {
            var splitted = strval.Trim('~')
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
            var label = splitted[0];
            var defaultValue = splitted[1];
            var type = splitted[2] switch
            {
                "0" => "str",
                "1" => "bool",
                "2" => "folder", // TODO - file or folder? Not determined?
                "3" => "int",
                "4" => "float",
                _ => throw new FormatException($"Unknown dynamic input type {splitted[2]}")
            };

            result = new DynamicElementSetup(
                Type: StringToValType(type),
                Default: ConvertToGivenType(StringToValType(type), defaultValue),
                DisplayName: label
            );
        }

        result = null;
        return false;
    }
    
    /// <summary>
    /// Recursively convert this configuration section to object hierarchy
    /// - Mappings are done by dictionaries
    /// - Collections are done by lists
    /// </summary>
    /// <param name="section"></param>
    /// <param name="inferTypes">Whether to attempt to convert primitive values to according types or leave all as strings</param>
    /// <returns></returns>
    public static object? ToObject(this IConfigurationSection section, bool inferTypes = true)
    {
        if (section.Value is not null)
        {
            if (inferTypes)
            {
                var info = InferTypeFromStringValue(section.Value);
                return ConvertToGivenType(info.Item1, section.Value);
            }
            return section.Value;
        }

        var children = section.GetChildren().ToList();
        if (children.Count == 0) return new Dictionary<string, object?>();

        // If the first child is a list, then we are dealing with a list
        if (children.First().Key == "0")
        {
            var list = new List<object?>();
            foreach (var child in children)
            {
                list.Add(child.ToObject(inferTypes));
            }

            return list;
        }

        // Otherwise we are dealing with a dictionary
        var dict = new Dictionary<string, object?>();
        foreach (var child in children)
        {
            dict[child.Key] = child.ToObject(inferTypes);
        }

        return dict;
    }
    
    public static Type StringToValType(string? typeAsString)
    {
        return typeAsString switch {
            "int" => typeof(int),
            "str" => typeof(string),
            "float" => typeof(float),
            "bool" => typeof(bool),
            "file" => typeof(string),
            "folder" => typeof(string),
            "strlist" => typeof(List<string>),
            _ => throw new ArgumentOutOfRangeException(nameof(typeAsString), typeAsString, null)};
    }

    public static (Type, object?) InferTypeFromStringValue(string? value)
    {
        if (value is null) return (typeof(string), null);
        // TODO - handle datetime / timespan
        if (int.TryParse(value, out var intv)) return (typeof(int), intv);
        if (float.TryParse(value, out var floatv)) return (typeof(float), floatv);
        if (bool.TryParse(value, out var boolv)) return (typeof(bool), boolv);
        
        return (typeof(string), value);
    }

    public static object? ConvertToGivenType(Type type, string? value)
    {
        if (value is null || type == typeof(string)) return value;
        
        // Use type converter
        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFrom(value);
        }

        throw new InvalidOperationException($"Cannot convert {value} to {type.Name}");
    }

    /// <summary>
    /// Apply values from dynamic form metadata to target object
    /// 
    /// Nesting is achieve by use . char in metadata keys.
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="target"></param>
    public static void ApplyValues(object? metadata, ref object? target)
    {
        
        void ApplyPropertyOrKeyValue(string key, object targetObj, DynamicElementSetup elementSetup)
        {
            var propertyInfo = targetObj.GetType().GetProperty(key);
            if (propertyInfo is null) return;
            
            // If source value type and target do not match, try to use type converter
            // Maybe del
            if (elementSetup.Default is not null && elementSetup.Type != propertyInfo.PropertyType)
            {
                var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                if (converter.CanConvertFrom(elementSetup.Default.GetType()))
                {
                    propertyInfo.SetValue(targetObj, converter.ConvertFrom(elementSetup.Default));
                }
                // Even if we are not able to do the conversion, we might still do one from string representation
                // For example target is DateTime and source is int (number of days)
                // However TimeSpan converter converts only string, therefore .ToString() on the value must be used
                else if (converter.CanConvertFrom(typeof(string)))
                {
                    propertyInfo.SetValue(targetObj, converter.ConvertFrom(elementSetup.Default.ToString()!));
                }
            }
            else
                propertyInfo.SetValue(targetObj, elementSetup.Default);
        }
        
        foreach (var kek in IterateDynamicInputs(metadata, target))
        {
            ApplyPropertyOrKeyValue(kek.key, kek.target, kek.meta);
        }
    }

    
    public record DynInputInfo(
        string Key,
        DynamicElementSetup Setup,
        object ValueTarget
    );
    
    public static IEnumerable<(string key, object target, DynamicElementSetup meta)> IterateDynamicInputs(IConfigurationSection metadata, object target)
    {
        object Nest(object on, string key)
        {
            var prop = on.GetType().GetProperty(key);
            if (prop is null) throw new InvalidOperationException($"Cannot nest {key} on {on.GetType().Name} since that property does not exist");
            var val = prop.GetValue(on);
            if (val is null) throw new InvalidOperationException($"Cannot nest {key} on {on.GetType().Name} since that property is empty");
            return val;
        }
        
        foreach (var meta in metadata.GetChildren())
        {
            // Should we nest or are we at property level?
            var keys = meta.Key.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var currentTarget = target;

            if (keys.Length > 1)
            {
                foreach (var nestKey in keys[..^1])
                {
                    currentTarget = Nest(currentTarget, nestKey);
                }
            }
            
            yield return (keys[^1], currentTarget, DynamicElementSetup.FromObject(meta));
        }
    }

    public static IEnumerable<DynInputInfo> PrepareDynamicInputs(IConfigurationSection metadata, object target)
    {
        foreach (var kek in IterateDynamicInputs(metadata, target))
        {
            // Only these that are editable 
            // For now this is no DisplayName provided
            
            if (string.IsNullOrEmpty(kek.meta.DisplayName)) continue;
            
            yield return new DynInputInfo(kek.key, kek.meta, kek.target);
        }
        
        
    }
    
}