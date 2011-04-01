// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Caching implementation using HttpRuntime.Cache.
//   Distinctive properties are application-lifecycle-long caching, application-wide scope and speed through in-process storage.
//   Suitable for storing volatile and non-volatile objects in a single-machine environment.
//   Suitable for storing non-volatile objects in a distributed application environment.
//   Not suitable for storing volatile objects in a distributed application environment.
//   Important: HttpRuntime.Cache manages its own memory usage, by throwing items out of cache indiscriminately when reaching a
//   certain memory threshold. This implementation therefore cannot guarantee that any item is available in cache
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Caching implementation using HttpRuntime.Cache.
    /// Distinctive properties are application-lifecycle-long caching, application-wide scope and speed through in-process storage.
    /// Suitable for storing volatile and non-volatile objects in a single-machine environment.
    /// Suitable for storing non-volatile objects in a distributed application environment.
    /// Not suitable for storing volatile objects in a distributed application environment.
    /// NOTE: HttpRuntime.Cache manages its own memory usage, by throwing items out of cache indiscriminately when reaching a
    /// NOTE: certain memory threshold. This implementation therefore cannot guarantee that any item is available in cache
    /// </summary>
    public class HttpRuntime : Dictionary
    {
        /// <summary>
        /// Method to return all cache keys
        /// </summary>
        /// <returns>A list of cache keys</returns>
        public override List<string> GetAllKeys()
        {
            // TODO: EROOT: Figure out a way to implement this. Since we can't get they keys and we can't index by int no way to iterate through
            throw new NotImplementedException();
        }


        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected override object GetItem(string key)
        {
            return System.Web.HttpRuntime.Cache[key];
        }

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add</param>
        protected override void SetItem(string key, object item)
        {
            System.Web.HttpRuntime.Cache.Insert(key, item);
        }

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected override void RemoveItem(string key)
        {
            System.Web.HttpRuntime.Cache.Remove(key);
        }        
    }
}
