// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Caching implementation using SharedCache distributed caching technology from IndexUs.
//   Distinctive properties are system-wide scope and relatively poor performance due to distributed nature.
//   Suitable for storing small volatile and non-volatile objects in a multi-machine environment.
//   Not ideal for storing large or complex objects as serialization and deserialization is expensive.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharedCache.WinServiceCommon.Formatters;
    using SharedCache.WinServiceCommon.Provider.Cache;

    /// <summary>
    /// Caching implementation using SharedCache distributed caching technology from IndexUs.
    /// Distinctive properties are system-wide scope and relatively poor performance due to distributed nature.
    /// Suitable for storing small volatile and non-volatile objects in a multi-machine environment.
    /// Not ideal for storing large or complex objects as serialization and deserialization is expensive.
    /// </summary>
    public sealed class Shared : Cache
    {
        /// <summary>
        /// The string to prefix to all keys. Helpful when unicity is needed.
        /// </summary>
        private readonly string KeyPrefix;

        /// <summary>
        /// An object that will be null if this object has not yet been instantiated.
        /// NOTE: This may actually be completely useless
        /// </summary>
        private static object isSetPlaceholder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shared"/> class.
        /// </summary>
        public Shared()
        {
            this.KeyPrefix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shared"/> class with a key prefix.
        /// Use this when operating multiple application instances within a single instance of SharedCache, to avoid collisions
        /// </summary>
        /// <param name="keyPrefix">The string to prefix to all keys.</param>
        public Shared(string keyPrefix)
        {
            this.KeyPrefix = keyPrefix;
        }

        /// <summary>
        /// Determine whether this object has been instantiated. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        /// <returns>A boolean indicating whether an instance of this object has been set</returns>
        public override bool IsInstanceSet()
        {
            return isSetPlaceholder != null;
        }

        /// <summary>
        /// Instantiate this object. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        public override void SetInstance()
        {
            isSetPlaceholder = new object();
        }

        /// <summary>
        /// Set a cache item to dirty state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetDirty(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set a cache item to clean state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetClean(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check whether a cached item with the given key has been marked as dirty
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether a cached item with the given key has been marked as dirty</returns>
        public override bool IsDirty(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to add to cache</param>
        public override void Add(string key, object item)
        {
            IndexusDistributionCache.SharedCache.Add(this.Prefix(key), item);
        }

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The dictionary of objects to add to cache, with cache key as key and item as value.</param>
        public override void Add(Dictionary<string, object> items)
        {
            // convert to binary
            var binaryItems = new Dictionary<string, byte[]>();
            foreach (var item in items)
            {
                binaryItems.Add(
                    this.Prefix(item.Key),
                    Serialization.BinarySerialize(item.Value));
            }

            IndexusDistributionCache.SharedCache.MultiAdd(binaryItems);
        }

        /// <summary>
        /// Gets an item from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached item</typeparam>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A cached item of type T</returns>
        public override T Get<T>(string key)
        {
            return IndexusDistributionCache.SharedCache.Get<T>(this.Prefix(key));
        }

        /// <summary>
        /// Get a number of items from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached items</typeparam>
        /// <param name="keys">Keys of the cache items</param>
        /// <returns>A dictionary of cached items of type T, where the dictionary keys correspond to those given</returns>
        public override Dictionary<string, T> MultiGet<T>(List<string> keys)
        {
            var uniqueKeys = keys;
            
            // Handles duplicate keys being used
            if (uniqueKeys != null)
            {
                uniqueKeys = uniqueKeys.Distinct().ToList().ConvertAll(key => this.Prefix(key));
            }
            
            // Retrieve cached items as byte arrays
            IDictionary<string, byte[]> multiGetResult;
            try
            {
                multiGetResult = IndexusDistributionCache.SharedCache.MultiGet(uniqueKeys);
            }
            catch (Exception e)
            {
                throw new Exception("Error retrieving items from shared cache. This may indicate that the server is not available", e);
            }

            // Build a dictionary of cached items of type T
            var multiGetTypedResult = new Dictionary<string, T>();
            foreach (var item in multiGetResult)
            {
                // Do not attempt conversion on null objects
                if (item.Value == null)
                {
                    continue;
                }

                // Deserialize cached item to type T and add to dictionary
                var value = Serialization.BinaryDeSerialize<T>(item.Value);
                multiGetTypedResult.Add(this.UnPrefix(item.Key), value);
            }

            return multiGetTypedResult;
        }

        /// <summary>
        /// Set the value for a key.
        /// Creates the key if it doesn't exist in the dictionary or sets its value and clear the dirty flag
        /// TODO: EROOT: calling this will throw an exception!
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to cache</param>
        public override void Set(string key, object item)
        {
            // Remove any existing item from cache
            if (this.Exists(key))
            {
                this.Clear(key);
            }

            // Add the item to cache
            this.Add(key, item);

            // Mark the item as clean
            // TODO: EROOT: calling this will throw an exception!
            this.SetClean(key);
        }

        /// <summary>
        /// Check whether a key exists in cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether an item with the given key exists in cache</returns>
        public override bool Exists(string key)
        {
            return this.GetAllKeys().Contains(key);
        }

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void Clear(string key)
        {
            IndexusDistributionCache.SharedCache.Remove(this.Prefix(key));
        }

        /// <summary>
        /// Remove multiple items from cache
        /// </summary>
        /// <param name="keys"> List of keys of the cached items </param>
        public override void MultiClear(List<string> keys)
        {
            var prefixedKeys = keys.ConvertAll(key => this.Prefix(key));

            IndexusDistributionCache.SharedCache.MultiDelete(prefixedKeys);
        }

        /// <summary>
        /// Remove all items from cach with keys matching the given regex
        /// </summary>
        /// <param name="keyRegex"> A regex indicating the keys of which items to remove </param>
        public override void RegexClear(string keyRegex)
        {
            // Insert prefix into regular expression
            var prefixedKeyRegex = keyRegex.Insert(
                keyRegex.StartsWith("^") ? 1 : 0, 
                System.Text.RegularExpressions.Regex.Escape(this.KeyPrefix));

            IndexusDistributionCache.SharedCache.RegexRemove(prefixedKeyRegex);
        }

        /// <summary>
        /// Get all the keys of items currently in cache
        /// </summary>
        /// <returns> All they cached item keys. </returns>
        public override List<string> GetAllKeys()
        {
            var prefixedKeys = IndexusDistributionCache.SharedCache.GetAllKeys();

            return prefixedKeys.ConvertAll(key => this.UnPrefix(key));
        }

        /// <summary>
        /// Prefix the given key with the configured prefix
        /// </summary>
        /// <param name="key">The key to prefix</param>
        /// <returns>The given key prefixed with the configured prefix</returns>
        private string Prefix(string key)
        {
            return String.Format("{0}{1}", this.KeyPrefix, key);
        }

        /// <summary>
        /// Remove the configured prefix from the given key
        /// </summary>
        /// <param name="key">The prefixed key from which to remove the prefix</param>
        /// <returns>The given string with the configured prefix removed</returns>
        private string UnPrefix(string key)
        {
            return key.StartsWith(this.KeyPrefix) ? key.Remove(0, this.KeyPrefix.Length) : key;
        }
    }
}