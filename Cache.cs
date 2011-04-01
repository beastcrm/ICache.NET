// <summary>
//   Defines the abstract base Cache type, from which all Cache implementations can derive.
//   Provides common functionality in the Test() and MultiAdd() methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the abstract base Cache type, from which all Cache implementations can derive.
    /// Provides common functionality in the Test() and MultiAdd() methods
    /// </summary>
    public abstract class Cache : ICache
    {
        /// <summary>
        /// Determine whether this object has been instantiated. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        /// <returns>A boolean indicating whether an instance of this object has been set</returns>
        public abstract bool IsInstanceSet();

        /// <summary>
        /// Instantiate this object. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        public abstract void SetInstance();

        /// <summary>
        /// Returns true if the cache is working
        /// </summary>
        /// <returns>A boolean indicating whether the cache is working</returns>
        public bool Test()
        {
            const string CacheKey = "test";
            const string CacheValue = "test";

            // retrieve value from cache if it's there
            var result = this.Get<string>(CacheKey);

            // if the value is not in cache, add it
            if (string.IsNullOrEmpty(result))
            {
                this.Add(CacheKey, CacheValue);
            }

            // check that the value from cache is the same as what we just put in
            return this.Get<string>(CacheKey) == CacheValue;
        }

        /// <summary>
        /// Set a cache item to dirty state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public abstract void SetDirty(string key);

        /// <summary>
        /// Set a cache item to clean state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public abstract void SetClean(string key);

        /// <summary>
        /// Checks whether a cached item with the given key has been marked as dirty
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether a cached item with the given key has been marked as dirty</returns>
        public abstract bool IsDirty(string key);

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to add</param>
        public abstract void Add(string key, object item);

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The dictionary of objects to add to cache, with cache key as key and item as value.</param>
        public abstract void Add(Dictionary<string, object> items);

        /// <summary>
        /// Add multiple ICacheable items to the cache
        /// </summary>
        /// <param name="collection">The collection of ICacheable objects to add.</param>
        /// <param name="cacheKeyPrefix">The prefix string to pass to the ICacheable's CacheKey method</param>
        public void Add(IEnumerable<ICacheable> collection, string cacheKeyPrefix)
        {
            // Build a dictionary of serializable items
            var cacheable = new Dictionary<string, object>();
            foreach (var item in collection)
            {
                cacheable.Add(item.CacheKey(cacheKeyPrefix), item);
            }

            // cache the dictionary
            this.Add(cacheable);
        }

        /// <summary>
        /// Gets an item from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached item</typeparam>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A cached item of type T</returns>
        public abstract T Get<T>(string key);

        /// <summary>
        /// Gets a number of items from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached items</typeparam>
        /// <param name="keys">Keys of the cache items</param>
        /// <returns>A dictionary of cached items of type T, where the dictionary keys correspond to those given</returns>
        public abstract Dictionary<string, T> MultiGet<T>(List<string> keys) where T : class;

        /// <summary>
        /// Set the value for a key.
        /// Creates the key if it doesn't exist in the dictionary or sets its value and clear the dirty flag
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to cache</param>
        public abstract void Set(string key, object item);

        /// <summary>
        /// Checks whether a key exists in cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether an item with the given key exists in cache</returns>
        public abstract bool Exists(string key);

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public abstract void Clear(string key);

        /// <summary>
        /// Remove multiple items from cache
        /// </summary>
        /// <param name="keys"> List of keys of the cached items </param>
        public abstract void MultiClear(List<string> keys);

        /// <summary>
        /// Remove all items from cach with key matching the given regex
        /// </summary>
        /// <param name="keyRegex"> A regex indicating the keys of which items to remove </param>
        public abstract void RegexClear(string keyRegex);

        /// <summary>
        /// Get all the keys of items currently in cache
        /// </summary>
        /// <returns> All they cached item keys. </returns>
        public abstract List<string> GetAllKeys();
    }
}