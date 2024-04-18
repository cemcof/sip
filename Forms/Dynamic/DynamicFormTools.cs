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
    int Min = int.MinValue,
    int Max = int.MaxValue	
)
{
    public static DynamicElementSetup FromObject(object setup)
    {
        DynamicElementSetup ParseConfigurationSectionSetup(IConfigurationSection section)
        {
            // First need to get the type and default value
            var specifiedType = section.GetSection(nameof(Type)).Value ?? "str";
            var type = DynamicFormTools.StringToValType(specifiedType);
            
            var defaultSection = section.GetSection(nameof(Default));
            object? defaultValue = (defaultSection.IsSectionList()) ?
                defaultSection.ToList() :
                DynamicFormTools.ConvertToGivenType(type, defaultSection.Value);
            
            // We also need to parse selection if any
            var selectionSection = section.GetSection(nameof(Selection));
            IList? selectionList = null; 
            if (selectionSection.IsSectionList())
            {
                selectionList = selectionSection.ToList();
            }
            
            // Now parse the rest of the setup
            return new DynamicElementSetup(
                Type: type,
                Default: defaultValue,
                SpecifiedType: specifiedType,
                DisplayName: section.GetSection(nameof(DisplayName)).Value,
                Selection: selectionList,
                Tip: section.GetSection(nameof(Tip)).Value,
                IsReadonly: section.GetSection(nameof(IsReadonly)).Get<bool>(),
                IsRequired: section.GetSection(nameof(IsRequired)).Get<bool>(),
                Unit: section.GetSection(nameof(Unit)).Value,
                Scope: section.GetSection(nameof(Scope)).Value,
                Flex: section.GetSection(nameof(Flex)).Value,
                Min: section.GetSection(nameof(Min)).Get<int>(),
                Max: section.GetSection(nameof(Max)).Get<int>()
            );

        }
        
        if (setup is DynamicElementSetup dyn) return dyn;

        if (setup is IConfigurationSection sect)
        {
            if (sect.Value is null && sect.GetChildren().Any())
            {
                if (sect.IsSectionList())
                {
                    // The sect is list, which is not configuration, but a default value
                    var list = sect.ToList();
                    return new DynamicElementSetup(Type: list.GetType(), Default: list);
                }
                else
                {
                    // The sect is a nested configuration, not a value
                    return ParseConfigurationSectionSetup(sect);
                }
            }
            else
            {
                // The sect is actually a value (nonlist)
                
                // Try to first parse it as a setup
                if (DynamicFormTools.TryParseSetupFromString(sect.Value, out var dynSetup))
                    return dynSetup!;
                
                // Now we know its really just a value
                var info = DynamicFormTools.InferTypeFromStringValue(sect.Value);
                return new DynamicElementSetup(Type: info.Item1, Default: info.Item2);
            }
        }
        
        throw new NotSupportedException	($"Type {setup.GetType().Name} is not supported as dynamic form setup");
    }
    
    
}

public interface IBindPoint
{
    
}

public class KBindPoint(ref string key)
{
    
}



public record BindPoint
{
    public object Target { get; init; }
    public string? Key { get; init; }
    
    public BindPoint(object Target, string? Key = null)
    {
        this.Target = Target;
        this.Key = Key;    
    }

    public object? GetValue()
    {
        if (Target is IDictionary dict)
        {
            return dict[Key];
        }
        else if (Target is IList list)
        {
            return list[int.Parse(Key!)];
        }
        else
        {
            // Target is arbitrary object - get it's property
        }
        
    }


    public Type? GetActualValueType()
     => GetValue()?.GetType();

    /// <summary>
    /// Terminal values are:
    /// - A primitive value
    /// - List of primitives
    /// - DynamicElementSetup
    /// - Dictionary that resembles dynamic element setup
    /// </summary>
    /// <returns></returns>
    public bool HasTerminalValue()
    {
        var valType = GetActualValueType();
        throw new NotImplementedException();
    }

    public Type GetValueType()
    {
        if (string.IsNullOrEmpty(Key)) return Target.GetType();
        

    }

    public static IEnumerable<BindPoint> EnumerateObject(object target)
    {
        if (target is IList tlist)
        {
            for (var i = 0; i < tlist.Count; i++)
            {
                yield return new BindPoint(tlist[i], i.ToString());
            }
        }
        else if (target is IDictionary tdict)
        {
            foreach (DictionaryEntry entry in tdict)
            {
                yield return new BindPoint(entry.Value, entry.Key.ToString());
            }
        }
        else
        {
            yield return new BindPoint(target, "");
            
        } 
    }

    /// <summary>
    /// Sets target object according to this BindPoint 
    /// </summary>
    /// <param name="target"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void EnsureTarget(ref object? target)
    {
        if (target is null)
        {
            if (Key is null)
            {
                throw new InvalidOperationException("Cannot ensure target without key");
            }
            
            if (Target is IList)
            {
                target = new List<object?>();
            }
            else if (Target is IDictionary)
            {
                target = new Dictionary<string, object?>();
            }
            else
            {
                throw new InvalidOperationException("Cannot ensure target without key");
            }
        }
    }
}


public class DynamicBindContext
{
    public DynamicBindContext(object source, object? target = null, string? listIdKey = null)
    {
        
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
    
    /// <summary>
    /// Iterate structure of source object, which can consist of lists, dictionaries or ordinary objects.
    /// Yield binding points of terminal primitive values or DynamicElementSetups.
    /// Recursively create and apply same structure into the target object, if possible.
    /// If dealing with lists, use listIdKey to identify the list items. 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<BindPoint> ApplyValues(BindPoint source, BindPoint target)
    {
        // If we should enumerate, recurse
        // Otherwise apply
        // How do we know? 
        // Well, by checking if source has terminal value or not
        // Terminal can be primitive value, null or DynamicElementSetup
    }

    public IEnumerable<BindPoint> ApplyList(IList source, object? target) 
    {
        
        
    }
    
    
    
    
}

public static class DynamicFormTools
{
    public const string ID_KEY = "ID";
    public const string TYPE_KEY = "TYPE";
    public const string DESCRIPTION_KEY = "DESCRIPTION";
    public const string NAME_KEY = "NAME";
    public const string GROUP_NAME_KEY = "GROUP_NAME";
    
    /// <summary>
    /// Recursively pa
    /// </summary>
    /// <param name="target"></param>
    public static void ParseDynamicSetups(object target)
    {
        var ctx = new DynamicBindContext(target);

    }
    
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
    
    public static bool IsSectionList(this IConfigurationSection section)
    {
        if (section.Value is not null) return false;
        var firstChild = section.GetChildren().FirstOrDefault();
        if (firstChild is null) return false;
        return int.TryParse(firstChild.Key, out _);
    }
    
    public static IList ToList(this IConfigurationSection section)
    {
        var subs = section.GetChildren().ToList();
        var listItemType = InferTypeFromStringValue(subs.First().Value);
        // Now create the list
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType.Item1))!;
        // Add items to the list
        foreach (var configurationSection in subs)
        {
            list.Add(InferTypeFromStringValue(configurationSection.Value).Item2);
        }

        return list;
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
    public static void ApplyValues(IConfigurationSection metadata, object target)
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