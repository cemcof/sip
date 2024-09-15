using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

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
    int Order = int.MaxValue,
    double Min = double.MinValue,
    double Max = double.MaxValue,
    double Step = 1.0,
    string? Filter = null
)
{
    public static DynamicElementSetup FromObject(object? setup, Type? targetType = null)
    {
        DynamicElementSetup result;

        if (setup is DynamicElementSetup dyn)
            result = dyn;
        else if (setup is string st && DynamicFormTools.TryParseSetupFromString(st, out var dynSetup))
            result = dynSetup!;
        else if (setup is IDictionary dict && 
            (dict.Contains(nameof(Type)) || dict.Contains(nameof(Default)))) // TODO - more robust
        {
            object? defaultVal = dict.PickValue<object>(nameof(Default));
            Type type = (dict.Contains(nameof(Type)))
                ? DynamicFormTools.StringToValType(dict.PickValue<string>(nameof(Type)))
                : DynamicFormTools.GetDynValueType(defaultVal);
            result = new DynamicElementSetup(
                Type: type,
                Default: defaultVal,
                SpecifiedType: dict.PickValue<string>(nameof(Type)),
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
                Group: dict.PickValue(nameof(Group), string.Empty)!,
                Order: dict.PickValue<int>(nameof(Order), int.MaxValue),
                GroupDesc: dict.PickValue(nameof(GroupDesc), string.Empty)!,
                Filter: dict.PickValue<string>(nameof(Filter))
            );
        }
        else
        {
            result = new DynamicElementSetup(
                Type: targetType ?? DynamicFormTools.GetDynValueType(setup), 
                Default: setup);
        }

        targetType ??= result.Type;
        var defaultV = DynamicFormTools.ConvertToSupportedIfNecessary(result.Default, targetType);
        targetType = defaultV?.GetType() ?? targetType;
        return result with { Default = defaultV, Type = targetType };
    }
}

public interface IBindPoint
{
    public object? GetValue();
    public void SetValue(object? value);

    public T GetValue<T>(T defaultValue = default);
    void SetDefault(object? val);
    object Target { get; }
    string Key { get; }
}

public abstract class BindPoint : IBindPoint
{
    public abstract object? GetValue();
    public abstract T GetValue<T>(T defaultValue = default);
    public abstract void SetValue(object? value);
    public abstract void SetDefault(object? val); 
    public abstract object Target { get; }
    public abstract string Key { get; }
    public static IBindPoint From(object o, object key)
    {
        return o switch
        {
            IDictionary dict => new DictBindPoint(dict, key.ToString()!),
            IList list => new ListBindPoint(list, (int)key),
            _ => new ObjectBindPoint(o, key.ToString()!)
        };
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
        if (!dict.Contains(key) || dict[key] is null) 
        {
            dict[key] = val;
        }
    }

    public override object Target => dict;
    public override string Key => key;
}


public class ObjectBindPoint(object obj, string key) : BindPoint
{
    public override object? GetValue()
    {
        return PropertyInfo?.GetValue(obj);
    }

    public override void SetValue(object? value)
    {
        PropertyInfo?.SetValue(obj, value);
    }

    public override T GetValue<T>(T defaultValue = default)
    {
        if (GetValue() is T val) return val;
        return defaultValue;
    }
    
    public override void SetDefault(object? val)
    {
        if (PropertyInfo is null) return;

        if (ObjectExtensions.IsDefault(GetValue()))
            SetValue(val);
    }
    
    public PropertyInfo? PropertyInfo => obj.GetType().GetProperty(key);

    public override object Target => obj;
    public override string Key => key;
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
    
    public override void SetDefault(object? val)
    {
        if (list[index] is null) list[index] = val;
    }

    public override object Target => list;
    public override string Key => index.ToString();
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

            return true;
        }

        result = null;
        return false;
    }

    public static Type GetDynValueType(object? value)
    {
        if (value is null) return typeof(string);
        // If it is a list of objects, then we need to check the type of the first element and set specific list accordingly
        if (value is List<object> list)
        {
            var subtype = GetDynValueType(list.FirstOrDefault());
            return typeof(List<>).MakeGenericType(subtype);
        }

        return value.GetType();
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
            "organization" => typeof(Organization),
            _ => throw new ArgumentOutOfRangeException(nameof(typeAsString), typeAsString, null)};
    }

    public static bool IsSimpleTypeSupported(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive ||
               t == typeof(string) ||
               t.IsEnum ||
               t == typeof(DateTime) ||
               t == typeof(TimeSpan) || 
               t == typeof(Organization);
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
    
    public static object? ConvertToSupportedIfNecessary(object? value, Type type)
    {
        // Handle list of values
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var innerType = type.GetGenericArguments()[0];
            if (!IsSimpleTypeSupported(innerType))
                throw new NotSupportedException(
                    $"Only simple types are supported for conversion, not {innerType.Name}, value: {value}");
            
            var convertedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(innerType))!;
            if (value is IList vallist)
            {
                foreach (var item in vallist)
                {
                    convertedList.Add(ConvertToSupportedIfNecessary(item, innerType));
                }
            }

            return convertedList;
        }
        
        if (!IsSimpleTypeSupported(type))
            throw new NotSupportedException(
                $"Only simple types are supported for conversion, not {type.Name}, value: {value}");
        
        if (value is null)
        {
            return (type.IsValueType) ? 
                Activator.CreateInstance(type) : 
                null;
        }
        
        
        if (value.GetType() == type) return value;
        
        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return converter.ConvertFrom(value);
        }
        // Even if we are not able to do the conversion, we might still do one from string representation
        // For example target is DateTime and source is int (number of days)
        // However TimeSpan converter converts only string, therefore .ToString() on the value must be used
        else if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFrom(value.ToString()!);
        }

        throw new NotSupportedException($"Cannot convert {value} to {type.Name}");
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
                    .Item1?
                    .GroupDesc;
                
                yield return new ElementGroup(groupGuys, group.Key, desc);
            }
        }
    };
    
    public static InspectResult DynamicInspect(
        object? metadata,
        object? target = null,
        bool forceSet = true
    )
    {
        var fakeTarget = new Dictionary<string, object?>() { { "key", target } };
        var bp = BindPoint.From(fakeTarget, "key");
        var result = new List<(DynamicElementSetup, IBindPoint)>();
        DynamicInspect(metadata, bp, result, forceSet);
        return new InspectResult(fakeTarget["key"]!, result.OrderBy(d => d.Item1.Order).ToList());
    }
    
    // TODO - rework this madness
    public static void DynamicInspect(
        object? metadata, 
        IBindPoint target,
        
        List<(DynamicElementSetup, IBindPoint)> resultElements,
        bool forceSet = false,
        string? listIdKey = ID_KEY
        )
    {
        try
        {
            var targetType = (target is ObjectBindPoint obp) ? obp.PropertyInfo?.PropertyType : null;
            Debug.WriteLine($"Dyn: {target.Key}, meta {metadata}, targetType: {targetType?.Name}");
            var dynElement = DynamicElementSetup.FromObject(metadata, targetType);
            // We are dealing with terminal value
            if (forceSet) target.SetValue(dynElement.Default);
            else target.SetDefault(dynElement.Default);
            Debug.WriteLine($"Set def terminal: {target.Key} to {dynElement.Default} after is: {target.GetValue()}");
            resultElements.Add((dynElement, target));
        }
        catch (NotSupportedException ex)
        {
            // We are dealing with a collection or mapping and need to recurse further
            if (metadata is IDictionary mdict)
            {
                // Ensure we have target object
                target.SetDefault(new Dictionary<string, object?>());
                var value = target.GetValue();
                Debug.Assert(value is not null);
                
                // Iterate the dictionary keys and recurse
                foreach (DictionaryEntry entry in mdict)
                {
                    var key = entry.Key.ToString()!;
                    var newMeta = entry.Value;
                    var newTarget = BindPoint.From(value, key);
                    DynamicInspect(newMeta, newTarget, resultElements, forceSet, listIdKey);
                }
                return;
            }
            
            if (metadata is IList mlist)
            {
                // Currently supported and implemented only lists of dictionaries
                
                // Ensure we have target object
                target.SetDefault(new List<object?>());
                var targetValue = (IList)(target.GetValue() ?? throw new InvalidOperationException());
                
                // Iterate the list and recurse
                for (var i = 0; i < mlist.Count; i++)
                {
                    var sourceItem = mlist[i] as IDictionary;
                    if (sourceItem is null) continue; // Only dict supported
                    // Check if in the target there is element that match through listIdKey
                    var tindex = -1;
                    IDictionary? tdict = null;
                    // Try to find target dictionary by listIdKey
                    if (listIdKey is not null && sourceItem.Contains(listIdKey))
                    {
                        // Iterate target value list
                        for (int j = 0; j < targetValue.Count; j++)
                        {
                            var item = targetValue[j]!;
                            var bp = BindPoint.From(item, listIdKey);
                            var bpval = bp.GetValue();
                            if (bpval is not null && bpval.Equals(sourceItem[listIdKey]))
                            {
                                tindex = j;
                                tdict = (IDictionary)item;
                                break; 
                            }
                        }
                    }
                    
                    // We didn't find it 
                    if (tdict is null)
                    {
                        targetValue.Add(new Dictionary<string, object>());
                        tindex = targetValue.Count - 1;
                    }
                    
                    var newMeta = mlist[i];
                    var newTarget = BindPoint.From(targetValue, tindex);
                    DynamicInspect(newMeta, newTarget, resultElements, forceSet, listIdKey);
                }
                
                return;
            }

            throw new InvalidOperationException($"Could not inspect/bind {metadata} to {target.Key}", ex);
        }
        
        
    }

    
}