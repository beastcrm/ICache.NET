// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Caching implementation using HttpContext.Current.Session.
//   Distinctive properties are per-user caching and machine-wide or system-wide (out of process) scope depending on app's SessionState configuration
//   Suitable for storing volatile and non-volatile user-specific objects.
//   Not ideal for storing large or complex objects in a distributed application environment as serialization and deserialization is expensive.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// Caching implementation using HttpContext.Current.Session.
    /// Distinctive properties are per-user caching and machine-wide or system-wide (out of process) scope depending on app's SessionState configuration
    /// Suitable for storing volatile and non-volatile user-specific objects.
    /// Not ideal for storing large or complex objects in a distributed application environment as serialization and deserialization is expensive.
    /// </summary>
    public class Session : Dictionary
    {
        /// <summary>
        /// Get all the keys of items currently in cache
        /// </summary>
        /// <returns> All they cached item keys. </returns>
        public override List<string> GetAllKeys()
        {
            // NOTE: Since these keys are stored in a KeysCollection object and there is no way to retreive them all at once, so must iterate through them all
            var keyCount = HttpContext.Current.Session.Keys.Count;
            var keyList = new List<string>(keyCount);

            keyCount--;
            while (keyCount >= 0)
            {
                keyList.Add(HttpContext.Current.Session.Keys.Get(keyCount--));
            }
           
            return keyList;
        }

        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected override object GetItem(string key)
        {
            return HttpContext.Current.Session[key];
        }

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add</param>
        protected override void SetItem(string key, object item)
        {
            HttpContext.Current.Session[key] = item;
        }

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected override void RemoveItem(string key)
        {
            HttpContext.Current.Session.Remove(key);
        }
    }
}
