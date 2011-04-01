// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Cache interface, providing methods for adding, retrieving and removing objects from cache
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System.Collections.Generic;

    /// <summary>
    /// Cache interface, providing methods for adding, retrieving and removing objects from cache
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Determine whether this object has been instantiated. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        /// <returns>A boolean indicating whether an instance of this object has been set</returns>
        bool IsInstanceSet();

        /// <summary>
        /// Instantiate this object. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        void SetInstance();

        /// <summary>
        ///  Test whether the cache is working
        /// </summary>
        /// <returns>Returns true if the cache is working</returns>
        bool Test();

        /// <summary>
        /// Set a cache item to dirty state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        void SetDirty(string key);

        /// <summary>
        /// Set a cache item to clean state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        void SetClean(string key);

        /// <summary>
        /// Check whether a cached item with the given key has been marked as dirty
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether a cached item with the given key has been marked as dirty</returns>
        bool IsDirty(string key);

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to add</param>
        void Add(string key, object item);

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The dictionary of objects to add to cache, with cache key as key and item as value.</param>
        void Add(Dictionary<string, object> items);

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The collection of ICacheable items to add</param>
        /// <param name="cacheKeyPrefix">The prefix string to pass to the ICacheable's CacheKey method</param>
        void Add(IEnumerable<ICacheable> items, string cacheKeyPrefix);

        /// <summary>
        /// Get an item from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached item</typeparam>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A cached item of type T</returns>
        T Get<T>(string key);

        /// <summary>
        /// Get a number of items from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached items</typeparam>
        /// <param name="keys">Keys of the cache items</param>
        /// <returns>A dictionary of cached items of type T, where the dictionary keys correspond to those given</returns>
        Dictionary<string, T> MultiGet<T>(List<string> keys) where T : class;

        /// <summary>
        /// Set the value for a key.
        /// Creates the key if it doesn't exist in the dictionary or sets its value and clear the dirty flag
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to cache</param>
        void Set(string key, object item);

        /// <summary>
        /// Check whether a key exists in cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether an item with the given key exists in cache</returns>
        bool Exists(string key);

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        void Clear(string key);

        /// <summary>
        /// Remove multiple items from cache
        /// </summary>
        /// <param name="keys"> List of keys of the cached items </param>
        void MultiClear(List<string> keys);

        /// <summary>
        /// Remove all items from cach with key matching the given regex
        /// </summary>
        /// <param name="keyRegex"> A regex indicating the keys of which items to remove </param>
        void RegexClear(string keyRegex);

        /// <summary>
        /// Get all the keys of items currently in cache
        /// </summary>
        /// <returns> All they cached item keys. </returns>
        List<string> GetAllKeys();
    }
}