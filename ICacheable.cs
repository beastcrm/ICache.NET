// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   This interface can be implemented by classes that want to be cached by an ICache implementation.
//   Allows instances of such classes to identify their own cache keys, enabling some scenarios.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Library.Cache
{
    /// <summary>
    /// This interface can be implemented by classes that want to be cached by an ICache implementation.
    /// Allows instances of such classes to identify their own cache keys, enabling some scenarios.
    /// </summary>
    public interface ICacheable
    {
        /// <summary>
        /// Get a unique key that will identify the item in cache
        /// </summary>
        /// <param name="prefix">A string that should be prefixed to the cache key</param>
        /// <returns>Unique key that will identify the item in cache</returns>
        string CacheKey(string prefix);
    }
}
