using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography.Xml;

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


public static class DynamicFormTools
{
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

    public static void ApplyValues(
        IConfigurationSection metadata,
        List<Dictionary<string, object?>> target)
    {
        target.Clear();
        
        foreach (var meta in metadata.GetChildren())
        {
            var wf = new Dictionary<string, object?>();
            target.Add(wf);
            foreach (var setup in meta.GetChildren())
            {
                var setupDyn = DynamicElementSetup.FromObject(setup);
                wf[setup.Key] = setupDyn.Default;
            }
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