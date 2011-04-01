// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Beast KK" file="Mongo.cs">
//   2010
// </copyright>
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
    using Library.Mongo;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// Caching implementation using MongoDB document db.
    /// Distinctive properties are permanent storage (no FIFO and can store to disk), system-wide scope and relatively poor performance due to distributed nature.
    /// Suitable for storing small volatile and non-volatile objects in a multi-machine environment.
    /// Not ideal for storing large or complex objects as serialization and deserialization is expensive(?)
    /// </summary>
    public sealed class Mongo : Cache
    {
        #region Private Properties

        /// <summary>
        /// Provides access to Mongo servers, databases and collections.
        /// </summary>
        private readonly Provider<MongoCacheItem> mongoProvider;

        /// <summary>
        /// The prefix for the MongoDB collections that hold cached data.
        /// </summary>
        private const string CollectionName = "cache";

        /// <summary>
        /// The string to prefix to all keys. Helpful when unicity is needed.
        /// </summary>
        private readonly string keyPrefix;

        /// <summary>
        /// An object that will be null if this object has not yet been instantiated.
        /// NOTE: This may actually be completely useless
        /// </summary>
        private static object isSetPlaceholder;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to an instance of MongoDB.</param>
        /// <param name="databaseName">The database name.</param>
        public Mongo(string connectionString, string databaseName)
        {
            this.mongoProvider = new Provider<MongoCacheItem>(connectionString, databaseName, CollectionName);

            this.keyPrefix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class with a key prefix.
        /// Use this when operating multiple application instances within a single instance of SharedCache, to avoid collisions
        /// </summary>
        /// <param name="connectionString">The connection string to connect to an instance of MongoDB.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="keyPrefix">The string to prefix to all keys.</param>
        public Mongo(string connectionString, string databaseName, string keyPrefix)
        {
            this.mongoProvider = new Provider<MongoCacheItem>(connectionString, databaseName, CollectionName);

            this.keyPrefix = keyPrefix;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set a cache item to dirty state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetDirty(string key)
        {
            var cacheItem = this.GetCacheItem(key);

            if (cacheItem != null)
            {
                cacheItem.IsDirty = true;
                this.mongoProvider.DefaultCollection.Save(cacheItem);
            }
        }

        /// <summary>
        /// Set a cache item to clean state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetClean(string key)
        {
            var cacheItem = this.GetCacheItem(key);

            if (cacheItem != null)
            {
                cacheItem.IsDirty = false;
                this.mongoProvider.DefaultCollection.Save(cacheItem);
            }
        }

        /// <summary>
        /// Check whether a cached item with the given key has been marked as dirty
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether a cached item with the given key has been marked as dirty</returns>
        public override bool IsDirty(string key)
        {
            var cacheItem = this.GetCacheItem(key);

            return cacheItem != null && cacheItem.IsDirty;
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to add to cache</param>
        public override void Add(string key, object item)
        {
            var cacheItem = new MongoCacheItem
            {
                Key = this.Prefix(key),
                Item = item,
                IsDirty = false
            };

            this.mongoProvider.DefaultCollection.Save(cacheItem);
        }

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The dictionary of objects to add to cache, with cache key as key and item as value.</param>
        public override void Add(Dictionary<string, object> items)
        {
            // Convert all items to cache items
            var cacheItems = new List<MongoCacheItem>();
            foreach (var item in items)
            {
                cacheItems.Add(new MongoCacheItem
                {
                    Key = this.Prefix(item.Key), 
                    Item = item.Value, 
                    IsDirty = false
                });
            }

            // Remove any possible dupes first (because there's no SaveBatch(), and InsertBatch() explodes on attempting to insert a duplicate id)
            var prefixedKeys = items.Keys.ToList().ConvertAll(key => this.Prefix(key));
            var query = Query.EQ("_id", BsonArray.Create(prefixedKeys));
            this.mongoProvider.DefaultCollection.Remove(query);

            this.mongoProvider.DefaultCollection.InsertBatch(cacheItems, SafeMode.False);
        }

        /// <summary>
        /// Gets an item from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached item</typeparam>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A cached item of type T</returns>
        public override T Get<T>(string key)
        {
            // Ensure MongoDB knows what our Type is for deserialization
            RegisterClassMap<T>();

            var cacheItem = this.GetCacheItem(key);

            return cacheItem != null && cacheItem.Item is T ? (T) cacheItem.Item : default(T);
        }

        /// <summary>
        /// Ensure MongoDB knows what our Type is for deserialization
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        private static void RegisterClassMap<T>()
        {
            if (BsonClassMap.LookupClassMap(typeof(T)) == null)
            {
                BsonClassMap.RegisterClassMap<T>();
            }
        }

        /// <summary>
        /// Get a number of items from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached items</typeparam>
        /// <param name="keys">Keys of the cache items</param>
        /// <returns>A dictionary of cached items of type T, where the dictionary keys correspond to those given</returns>
        public override Dictionary<string, T> MultiGet<T>(List<string> keys)
        {
            // Ensure MongoDB knows what our Type is for deserialization
            RegisterClassMap<T>();

            var uniqueKeys = keys;
            
            // Handles duplicate keys being used
            if (uniqueKeys != null)
            {
                uniqueKeys = uniqueKeys.Distinct().ToList().ConvertAll(key => this.Prefix(key));
            }

            // Retrieve cached items as byte arrays
            List<MongoCacheItem> multiGetResult;
            //try
            //{
                var query = Query.In("_id", BsonArray.Create(uniqueKeys));
                multiGetResult = this.mongoProvider.DefaultCollection.FindAs<MongoCacheItem>(query).ToList();
            //}
            //catch (Exception e)
            //{
                //throw new Exception("Error retrieving items from mongo cache. This may indicate that the server is not available", e);
            //}

            // Build a dictionary of cached items of type T
            var multiGetTypedResult = new Dictionary<string, T>();
            foreach (var cacheItem in multiGetResult)
            {
                // Do not attempt conversion on null objects
                if (cacheItem.Item == null)
                {
                    continue;
                }

                // Cast cached item to type T and add to dictionary
                var value = cacheItem.Item is T ? (T)cacheItem.Item : default(T);
                multiGetTypedResult.Add(this.UnPrefix(cacheItem.Key), value);
            }

            return multiGetTypedResult;
        }

        /// <summary>
        /// Set the value for a key.
        /// Creates the key if it doesn't exist in the dictionary or sets its value and clear the dirty flag
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to cache</param>
        public override void Set(string key, object item)
        {
            // The Add also sets if it already exists
            this.Add(key, item);
        }

        /// <summary>
        /// Check whether a key exists in cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether an item with the given key exists in cache</returns>
        public override bool Exists(string key)
        {
            var query = Query.EQ("_id", this.Prefix(key));
            return this.mongoProvider.DefaultCollection.Count(query) > 0;
        }

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void Clear(string key)
        {
            var query = Query.EQ("_id", this.Prefix(key));
            this.mongoProvider.DefaultCollection.Remove(query);
        }

        /// <summary>
        /// Remove multiple items from cache
        /// </summary>
        /// <param name="keys"> List of keys of the cached items </param>
        public override void MultiClear(List<string> keys)
        {
            var prefixedKeys = keys.ConvertAll(key => this.Prefix(key));

            var query = Query.In("_id", BsonArray.Create(prefixedKeys));
            this.mongoProvider.DefaultCollection.Remove(query);
        }

        /// <summary>
        /// Remove all items from cache with keys matching the given regex
        /// </summary>
        /// <param name="keyRegex"> A regex indicating the keys of which items to remove </param>
        public override void RegexClear(string keyRegex)
        {
            // Insert prefix into regular expression
            var prefixedKeyRegex = keyRegex.Insert(
                keyRegex.StartsWith("^") ? 1 : 0, 
                System.Text.RegularExpressions.Regex.Escape(this.keyPrefix));

            var query = Query.Matches("_id", BsonRegularExpression.Create(prefixedKeyRegex));
            this.mongoProvider.DefaultCollection.Remove(query);
        }

        /// <summary>
        /// Get all the keys of items currently in cache
        /// </summary>
        /// <returns> All they cached item keys. </returns>
        public override List<string> GetAllKeys()
        {
            var prefixedKeys = this.mongoProvider.DefaultCollection
                .FindAll()
                .Select(cacheItem => cacheItem.Key)
                .ToList();

            return prefixedKeys.ConvertAll(key => this.UnPrefix(key));
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Prefix the given key with the configured prefix
        /// </summary>
        /// <param name="key">The key to prefix</param>
        /// <returns>The given key prefixed with the configured prefix</returns>
        private string Prefix(string key)
        {
            return String.Format("{0}{1}", this.keyPrefix, key);
        }

        /// <summary>
        /// Remove the configured prefix from the given key
        /// </summary>
        /// <param name="key">The prefixed key from which to remove the prefix</param>
        /// <returns>The given string with the configured prefix removed</returns>
        private string UnPrefix(string key)
        {
            return key.StartsWith(this.keyPrefix) ? key.Remove(0, this.keyPrefix.Length) : key;
        }

        /// <summary>
        /// Get a cache item from cache in its wrapped form
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>The cache item if it exists, otherwise null</returns>
        private MongoCacheItem GetCacheItem(string key)
        {
            var query = Query.EQ("_id", this.Prefix(key));
            var cacheItems = this.mongoProvider.DefaultCollection.Find(query).SetLimit(1).ToList();

            return cacheItems.Count() > 0 ? cacheItems[0] : null;
        }

        #endregion

        #region Private Structures

        /// <summary>
        /// A single cache item, wrapping the actual item to be stored
        /// </summary>
        private sealed class MongoCacheItem
        {
            /// <summary>
            /// Gets or sets the unique key, stored with a prefix for ensuring unicity across multiple applications that want to use the same cache instance
            /// </summary>
            [BsonId]
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the object to store in cache
            /// </summary>
            public object Item { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this item is dirty
            /// </summary>
            public bool IsDirty { get; set; }
        }

        #endregion
    }
}