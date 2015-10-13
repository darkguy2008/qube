using System.Collections.Generic;
using System.Dynamic;

namespace Qube.Extensions
{
    public static class DictionaryExtensions
    {
        public static ExpandoObject ToDynamic(this IDictionary<string, object> dictionary)
        {
            var expando = (IDictionary<string, object>)new ExpandoObject();

            foreach (var item in dictionary)
            {
                var innerDictionary = item.Value as IDictionary<string, object>;
                if (innerDictionary != null)
                    expando.Add(item.Key, innerDictionary.ToDynamic());
                else
                    expando.Add(item.Key, item.Value);
            }

            return (ExpandoObject)expando;
        }

        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fallback = default(TValue))
        {
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : fallback;
        }
    }
}
