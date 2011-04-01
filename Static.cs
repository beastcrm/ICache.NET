// <summary>
//   Caching implementation using C# Static objects.
//   Distinctive properties are zero dependencies, application-lifecycle-long caching, application-wide scope and speed through in-process storage.
//   Suitable for storing volatile and non-volatile objects in a single-machine environment.
//   Suitable for storing non-volatile objects in a distributed application environment.
//   Not suitable for storing volatile objects in a distributed application environment.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Caching implementation using C# Static objects.
    /// Distinctive properties are zero dependencies, application-lifecycle-long caching, application-wide scope and speed through in-process storage.
    /// Suitable for storing volatile and non-volatile objects in a single-machine environment.
    /// Suitable for storing non-volatile objects in a distributed application environment.
    /// Not suitable for storing volatile objects in a distributed application environment.
    /// </summary>
    public class Static : Dictionary
    {
        /// <summary>
        /// store for all cached items
        /// </summary>
        private readonly Dictionary<string, object> store = new Dictionary<string, object>();

        /// <summary>
        /// A method to return all the keys of the cache
        /// </summary>
        /// <returns>All the cach keys</returns>
        public override List<string> GetAllKeys()
        {
            return new List<string>(this.store.Keys);
        }

        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected override object GetItem(string key)
        {
            return this.store.ContainsKey(key) ? this.store[key] : null;
        }

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add to the dictionary</param>
        protected override void SetItem(string key, object item)
        {
            this.store[key] = item;
        }

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected override void RemoveItem(string key)
        {
            this.store.Remove(key);
        }
    }
}
