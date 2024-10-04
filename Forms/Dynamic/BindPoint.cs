using System.Collections;

namespace sip.Forms.Dynamic;

public interface IBindPoint
{
    public object? GetValue();
    public T? GetValue<T>(T? defaultValue = default);
    
    public void SetValue(object? value);

    void SetDefault(object? val);
    object Target { get; }
    string Key { get; }
}

public abstract class BindPoint : IBindPoint
{
    public abstract object? GetValue();
    public abstract T? GetValue<T>(T? defaultValue = default);
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

    public override T? GetValue<T>(T? defaultValue = default) where T : default
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

    public override T? GetValue<T>(T? defaultValue = default) where T : default
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

    public override T? GetValue<T>(T? defaultValue = default) where T : default
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
