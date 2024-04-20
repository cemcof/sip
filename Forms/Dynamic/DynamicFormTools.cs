using System.Collections;
using System.ComponentModel;

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
    string Group = "",
    string GroupDesc = "",
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
            Type type;
            object? defaultVal = dict.PickValue<object>(nameof(Default));
            if (dict.Contains(nameof(Type)))
            {
                type = DynamicFormTools.StringToValType(dict.PickValue<string>(nameof(Type)));
            }
            else
            {
                var (t, supported) = GetTypeInfo(defaultVal);
                if (!supported) throw new NotSupportedException($"Type {t.Name} is not supported as dynamic form setup");
                type = t;
            }
            
            return new DynamicElementSetup(
                Type: type,
                Default: defaultVal,
                SpecifiedType: dict.PickValue<string>(nameof(DisplayName)),
                DisplayName: dict.PickValue<string>(nameof(DisplayName)),
                Selection: dict.PickValue<IList>(nameof(Selection)),
                Tip: dict.PickValue<string>(nameof(Tip)),
                IsReadonly: dict.PickValue<bool>(nameof(IsReadonly)),
                IsRequired: dict.PickValue<bool>(nameof(IsRequired)),
                Unit: dict.PickValue<string>(nameof(Unit)),
                Scope: dict.PickValue<string>(nameof(Scope)),
                Flex: dict.PickValue<string>(nameof(Flex)),
                Min: dict.PickValue<int>(nameof(Min)),
                Max: dict.PickValue<int>(nameof(Max)),
                Group: dict.PickValue<string>(nameof(Group), string.Empty)!,
                GroupDesc: dict.PickValue<string>(nameof(GroupDesc), string.Empty)!
            );
        }
        
        throw new NotSupportedException	($"Type {setup?.GetType().Name} is not supported as dynamic form setup");
    }
    
    
}

public interface IBindPoint
{
    public object? GetValue();
    public void SetValue(object? value);

    public T? GetValueByKey<T>(string key, T? defaultValue = default);
    public T GetValue<T>(T defaultValue = default);
    void SetDefault(object? val);
    object Target { get; }
    string Key { get; }
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

    public T? GetValueByKey<T>(string key, T? defaultValue = default)
    {
        throw new NotImplementedException();
    }

    public virtual T GetValue<T>(T defaultValue = default)
    {
        throw new NotSupportedException();
    }

    public virtual void SetDefault(object? val)
    {
        throw new NotImplementedException();
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
    
    public override void SetDefault(object? val)
    {
        // TODO - default val?
        if (!dict.Contains(key)) dict[key] = val;
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
    
    public override void SetDefault(object? val)
    {
        var prop = obj.GetType().GetProperty(key);
        if (prop is null || prop.GetValue(obj) is not null) return;

        if (val is not null && prop.PropertyType != val.GetType())
        {
            var converter = TypeDescriptor.GetConverter(prop.PropertyType);
            if (converter.CanConvertFrom(val.GetType()))
            {
                prop.SetValue(obj, converter.ConvertFrom(val));
            }
             // Even if we are not able to do the conversion, we might still do one from string representation
             // For example target is DateTime and source is int (number of days)
             // However TimeSpan converter converts only string, therefore .ToString() on the value must be used
            else if (converter.CanConvertFrom(typeof(string)))
            {
                prop.SetValue(obj, converter.ConvertFrom(val.ToString()!));
            } 
        }
        else
            // TODO - handle primitive types
            prop.SetValue(obj, val);
    }
}


public static class DynamicFormTools
{
    public const string ID_KEY = "object.id";
    
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

    public record ElementGroup(
        List<(DynamicElementSetup, IBindPoint)> Elements,
        string? Name = null,
        string? Description = null);

    public record InspectResult(
        object? Target,
        List<(DynamicElementSetup, IBindPoint)> Elements
    )
    {
        public IEnumerable<ElementGroup> ToGroups()
        {
            var groups = Elements.GroupBy(e => e.Item1.Group);
            foreach (var group in groups)
            {
                var groupGuys = group.ToList();
                var desc = groupGuys
                    .FirstOrDefault(g => !string.IsNullOrWhiteSpace(g.Item1.GroupDesc))
                    .Item1
                    .GroupDesc;
                
                yield return new ElementGroup(groupGuys, group.Key, desc);
            }
        }
    };
    
    public static InspectResult DynamicInspect(
        object? metadata,
        object? target
    )
    {
        var fakeTarget = new Dictionary<string, object?>() { { "key", target } };
        var bp = BindPoint.From(fakeTarget, "key");
        var result = new List<(DynamicElementSetup, IBindPoint)>();
        DynamicInspect(metadata, bp, result);
        return new InspectResult(fakeTarget["key"]!, result);
    }
    
    public static void DynamicInspect(
        object? metadata, 
        IBindPoint target,
        
        List<(DynamicElementSetup, IBindPoint)> re sultElements,
        string? listIdKey = ID_KEY
        )
    {
        try
        {
            var dynElement = DynamicElementSetup.FromObject(metadata);
            // We are dealing with terminal value
            target.SetDefault(dynElement.Default);
            resultElements.Add((dynElement, target));

        }
        catch (NotSupportedException e)
        {
            // We are dealing with a collection or mapping and need to recurse furhter
            if (metadata is IDictionary mdict)
            {
                // Ensure we have target object
                target.SetDefault<Dictionary<string, object>>(new Dictionary<string, object?>(), out var value);
                
                // Iterate the dictionary keys and recurse
                foreach (DictionaryEntry entry in mdict)
                {
                    var key = entry.Key.ToString();
                    var newMeta = entry.Value;
                    var newTarget = BindPoint.From(value, key);
                    DynamicInspect(newMeta, newTarget, resultElements, listIdKey);
                    return;
                }
            }
            
            if (metadata is IList mlist)
            {
                // Ensure we have target object
                // target.SetDefault<List<object>>(new List<object?>(), out var value);
                // List<object> targetValue = new List<object>();
                // Iterate the list and recurse
                // for (var i = 0; i < mlist.Count; i++)
                // {
                    // Check if in the target there is element that match through listIdKey
                    // TODO 
                    // if (listIdKey is not null)
                    // {
                    //     var item = targetValue.FirstOrDefault(i =>
                    //     {
                    //         var bp = BindPoint.From(i, listIdKey);
                    //         var bpval = bp.GetValue();
                    //         bp.Exists() && bpval is not null && bpval.Equals(mlist[i]); 
                    //     })
                    // }
                    // var newMeta = mlist[i];
                    // var newTarget = BindPoint.From(value, i);
                    // DynamicInspect(newMeta, newTarget, resultElements, listIdKey);
                // }
            }
        }
        
        
    }

    
}