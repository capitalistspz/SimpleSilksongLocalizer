using System.Collections.Generic;

namespace SimpleSilksongLocalizer;

public static class Extensions
{
    public static TValue GetOrInsertNew<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }

        return dict[key] = new TValue();
    }
}