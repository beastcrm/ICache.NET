// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Caching implementation using HttpContext.Current.Application.
//   Distinctive properties are application-lifecycle-long caching, application-wide scope and speed through in-process storage.
//   Suitable for storing volatile and non-volatile objects in a single-machine environment.
//   Suitable for storing non-volatile objects in a distributed application environment.
//   Not suitable for storing volatile objects in a distributed application environment.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// Caching implementation using HttpContext.Current.Application.
    /// Distinctive properties are application-lifecycle-long caching, application-wide scope and speed through in-process storage.
    /// Suitable for storing volatile and non-volatile objects in a single-machine environment.
    /// Suitable for storing non-volatile objects in a distributed application environment.
    /// Not suitable for storing volatile objects in a distributed application environment.
    /// </summary>
    public class Application : Dictionary
    {
        /// <summary>
        /// Return all the keys for this cache
        /// </summary>
        /// <returns> A list of keys </returns>
        public override List<string> GetAllKeys()
        {
            return new List<string>(HttpContext.Current.Application.AllKeys);            
        }

        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected override object GetItem(string key)
        {
            return HttpContext.Current.Application[key];
        }

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add</param>
        protected override void SetItem(string key, object item)
        {
            HttpContext.Current.Application[key] = item;
        }

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected override void RemoveItem(string key)
        {
            HttpContext.Current.Application.Remove(key);
        }
    }
}
