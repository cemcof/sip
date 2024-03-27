using System.Collections;

namespace sip.Utils;

public static class DictListUtils
{
    /// <summary>
    /// THIS IS NOT RECURSIVE!
    /// </summary>
    /// <param name="target"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="forceKey">If key does not exist on a target, create it and assign value anyways</param>
    public static void Populate(this List<Dictionary<string, object?>> target, string key, object? value, bool forceKey = false)
    {
        foreach (var dictionary in target)
        {
            if (dictionary.ContainsKey(key) || forceKey)
            {
                dictionary[key] = value;
            }
        }
    }

    // public static void ApplyDefaults(this Dictionary<string, object?> target,
    //     Dictionary<string, object?> source)
    // {
    //     foreach (var keyValuePair in source)
    //     {
    //         var val = keyValuePair.Value;
    //         if (keyValuePair.Value is IDictionary dictv)
    //         {
    //             val = dictv.PickValue<object>(keyValuePair.Key);
    //         }
    //
    //         if (!target.ContainsKey(keyValuePair.Key))
    //             target[keyValuePair.Key] = val;
    //
    //     }
    // }
    //
    


    /// <summary>
    /// Safely pick value of desired type on given key on a mixed-type dictionary.
    /// If key is not present or value type is invalid, return specified default value.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="key"></param>
    /// <param name="def"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? PickValue<T>(this Dictionary<string, object?> target, string key, T? def = default)
    {
        if (target.ContainsKey(key) && target[key] is T typedVal)
        {
            return typedVal;
        }
        
        return def;
    }
    
    public static T? PickValue<T>(this IDictionary target, string key, T? def = default)
    {
        if (target.Contains(key) && target[key] is T typedVal)
        {
            return typedVal;
        }
        
        return def;
    }
}