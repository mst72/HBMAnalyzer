using System;
using System.Collections.Generic;
using ElasticView.Helpers;

namespace ElasticView.Extensions
{
    public static class DictionaryExtensions
    {
        public static TItem Get<TKey, TItem>(this IDictionary<TKey, TItem> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            try
            {
                return dictionary[key];
            }
            catch (KeyNotFoundException)
            {
                var message = string.Format("Cannot get item by key '{0}'.", key);

                LogHelper.Log.Error(message);
                throw new InvalidOperationException(message);
            }
        }

        public static TItem GetOrCreate<TKey, TItem>(this IDictionary<TKey, TItem> dictionary, TKey key, Func<TItem> cerateItem)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            if (cerateItem == null) throw new ArgumentNullException("cerateItem");

            TItem item;
            if (!dictionary.TryGetValue(key, out item))
            {
                item = cerateItem();
                dictionary.Add(key, item);
            }

            return item;
        }

        public static TItem TryGet<TKey, TItem>(this IDictionary<TKey, TItem> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            TItem value;
            if (dictionary.TryGetValue(key, out value)) return value;

            LogHelper.Log.DebugFormat("Cannot find item in dictionary by key '{0}'.", key);

            return default(TItem);
        }
    }
}