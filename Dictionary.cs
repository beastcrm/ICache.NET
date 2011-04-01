// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Abstract caching implementation using any dictionary-like item
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Abstract caching implementation using any dictionary-like item
    /// </summary>
    public abstract class Dictionary : Cache
    {
        /// <summary>
        /// Dictionary key for the list that contains dirtied cache items
        /// </summary>
        private const string DirtyList = "DIRTY_ITEMS";

        /// <summary>
        /// Dictionary key for the object whose presence will indicate whether this object has been instantiated. 
        /// Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        private const string IsSetPlaceholder = "IS_SET_PLACEHOLDER";

        /// <summary>
        /// Determine whether this object has been instantiated. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        /// <returns>A boolean indicating whether an instance of this object has been set</returns>
        public override bool IsInstanceSet()
        {
            return this.GetItem(IsSetPlaceholder) != null;
        }

        /// <summary>
        /// Instantiate this object. Useful when using a static Cache instance.
        /// NOTE: This may actually be completely useless
        /// </summary>
        public override void SetInstance()
        {
            this.SetItem(IsSetPlaceholder, new object());
        }

        /// <summary>
        /// Set a cache item to dirty state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetDirty(string key)
        {
            if (this.GetItem(DirtyList) == null)
            {
                this.SetItem(DirtyList, new List<string>());
            }

            var dirtyItems = this.GetItem(DirtyList) as List<string>;

            if (dirtyItems != null)
            {
                dirtyItems.Add(key);
            }

            this.SetItem(DirtyList, dirtyItems);
        }

        /// <summary>
        /// Set a cache item to clean state
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void SetClean(string key)
        {
            if (this.GetItem(DirtyList) == null)
            {
                return;
            }

            var dirtyItems = this.GetItem(DirtyList) as List<string>;
            if (dirtyItems != null && dirtyItems.Contains(key))
            {
                dirtyItems.Remove(key);
            }

            this.SetItem(DirtyList, dirtyItems);
        }

        /// <summary>
        /// Checks whether a cached item with the given key has been marked as dirty
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether a cached item with the given key has been marked as dirty</returns>
        public override bool IsDirty(string key)
        {
            if (this.GetItem(DirtyList) != null)
            {
                var dirtyItems = this.GetItem(DirtyList) as List<string>;
                if (dirtyItems != null)
                {
                    return dirtyItems.Contains(key);
                }
            }

            return false;
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to add</param>
        public override void Add(string key, object item)
        {
            this.SetItem(key, item);
        }

        /// <summary>
        /// Add multiple items to the cache
        /// </summary>
        /// <param name="items">The dictionary of objects to add to cache, with cache key as key and item as value.</param>
        public override void Add(Dictionary<string, object> items)
        {
            foreach (var item in items)
            {
                this.SetItem(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Gets an item from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached item</typeparam>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A cached item of type T</returns>
        public override T Get<T>(string key)
        {
            return (T)this.GetItem(key);
        }

        /// <summary>
        /// Gets a number of items from cache
        /// </summary>
        /// <typeparam name="T">The type to which to cast the cached items</typeparam>
        /// <param name="keys">Keys of the cache items</param>
        /// <returns>A dictionary of cached items cast to type T, where the dictionary keys correspond to those given</returns>
        public override Dictionary<string, T> MultiGet<T>(List<string> keys)
        {
            // Dedupe list to ensure no duplicate keys are added to the dictionary
            var distinctKeys = keys.Distinct();

            var items = new Dictionary<string, T>();
            foreach (var key in distinctKeys)
            {
                var item = this.Get<T>(key);
                if (item != null)
                {
                    items.Add(key, item);
                }
            }

            return items;
        }

        /// <summary>
        /// Set the value for a key.
        /// Creates the key if it doesn't exist in the dictionary or sets its value and clear the dirty flag
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="item">Item to cache</param>
        public override void Set(string key, object item)
        {
            this.SetItem(key, item);
            this.SetClean(key);
        }

        /// <summary>
        /// Check whether a key exists in cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>A boolean indicating whether an item with the given key exists in cache</returns>
        public override bool Exists(string key)
        {
            return this.GetItem(key) != null;
        }

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        public override void Clear(string key)
        {
            this.RemoveItem(key);
        }

        /// <summary>
        /// Remove multiple items from cache
        /// </summary>
        /// <param name="keys"> List of keys of the cached items </param>
        public override void MultiClear(List<string> keys)
        {
            this.RemoveAll(keys);
        }

        /// <summary>
        /// Remove all items from cach with key matching the given regex
        /// </summary>
        /// <param name="keyRegex"> A regex indicating the keys of which items to remove </param>
        /// NOTE: Be careful when useing this. Peformance may be bad with large collections depending on child class implementation of GetAllKeys()
        public override void RegexClear(string keyRegex)
        {
            // Retrieve all keys
            var allKeys = this.GetAllKeys();

            // Define a regular expression from the given string
            var searchTerm = new System.Text.RegularExpressions.Regex(keyRegex);

            // Identify keys matching the regular expression
            var matchingKeys = new List<string>(allKeys.Where(key => searchTerm.Matches(key).Count > 0));

            // Remove all items for the matching keys
            this.RemoveAll(matchingKeys);
        }

        /// <summary> Removes a list of keys </summary>
        /// <param name="keys"> The keys to remove. </param>
        protected void RemoveAll(List<string> keys)
        {
            foreach (var key in keys)
            {
                this.RemoveItem(key);
            }
        }

        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected abstract object GetItem(string key);

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add</param>
        protected abstract void SetItem(string key, object item);

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected abstract void RemoveItem(string key);
    }
}
