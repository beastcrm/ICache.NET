// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Caching implementation using HttpContext.Current.
//   Distinctive properties are per-HTTP request caching and machine-wide scope.
//   Suitable for storing volatile and non-volatile request-specific objects.
//   Not suitable for storing objects that should exist longer than the HTTP request lifespan.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// Caching implementation using HttpContext.Current.
    /// Distinctive properties are per-HTTP request caching and machine-wide scope.
    /// Suitable for storing volatile and non-volatile request-specific objects.
    /// Not suitable for storing objects that should exist longer than the HTTP request lifespan.
    /// </summary>
    public class Request : Dictionary
    {
        /// <summary>
        /// A method to return all the cache keys
        /// </summary>
        /// <returns>The cache keys </returns>
        public override List<string> GetAllKeys()
        {
            var keyArray = new string[HttpContext.Current.Items.Keys.Count];
            HttpContext.Current.Items.Keys.CopyTo(keyArray, 0);

            // then create a list from the array (to avoid manually iterating through each one)
            var keyList = new List<string>(keyArray);
            return keyList;
        }

        /// <summary>
        /// Return an item from the dictionary by key
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <returns>An object from the dictionary for the given key</returns>
        protected override object GetItem(string key)
        {
            return HttpContext.Current.Items[key];
        }

        /// <summary>
        /// Add an item to the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        /// <param name="item">Item to add</param>
        protected override void SetItem(string key, object item)
        {
            HttpContext.Current.Items[key] = item;
        }

        /// <summary>
        /// Remove an item from the dictionary
        /// </summary>
        /// <param name="key">Key of item in dictionary</param>
        protected override void RemoveItem(string key)
        {
            HttpContext.Current.Items.Remove(key);
        }       
    }
}
