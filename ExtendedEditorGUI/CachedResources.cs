using System.Collections.Generic;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Extends the Resources class with the ability to cache resource references in a static dictionary and then access them at no extra cost.
    /// </summary>
    public static class CachedResources
    {
        private static Dictionary<Hash128, Object> storage = new Dictionary<Hash128, Object>();

        /// <summary>
        /// Computes a hash to identify the cached object.
        /// </summary>
        /// <typeparam name="T">Requested resource type</typeparam>
        /// <param name="path">Relative path in Resources folder</param>
        /// <returns></returns>
        private static Hash128 GetHash<T>(string path) => Hash128.Compute(typeof(T) + path);

        /// <summary>
        /// Allows you to get resources from the cache if they were previously accessed using this method. 
        /// Otherwise, it loads and caches the object for subsequent accesses. Recommended only for frequently used objects.
        /// </summary>
        /// <typeparam name="T">Requested resource type</typeparam>
        /// <param name="path">Relative path in Resources folder</param>
        /// <returns></returns>
        public static T Get<T>(string path) where T : Object
        {
            Hash128 hash = GetHash<T>(path);
            if (!storage.ContainsKey(hash))
            {
                storage.Add(hash, Load<T>(path));
            }
            return (T)storage[hash];
        }

        /// <summary>
        /// Allows you to get resources from the cache if they were previously accessed using this method.
        /// Otherwise, it loads and returns the object without caching.
        /// </summary>
        /// <typeparam name="T">Requested resource type</typeparam>
        /// <param name="path">Relative path in Resources folder</param>
        /// <returns></returns>
        public static T Load<T>(string path) where T : Object
        {
            Hash128 hash = GetHash<T>(path);
            if (storage.ContainsKey(hash)) return (T)storage[hash];

            string ext = System.IO.Path.GetExtension(path);
            var resource = Resources.Load<T>(string.IsNullOrEmpty(ext) ? path : path.Replace(ext, ""));
            if (resource) { return resource; }
            else { throw new System.Exception(string.Format("Resources.Load<{0}> - resource could not be found at the address: {1}", typeof(T), path)); }
        }

        /// <summary>
        /// A method for removing a reference to an object from the cache if it is no longer needed.
        /// </summary>
        /// <typeparam name="T">Requested resource type</typeparam>
        /// <param name="path">Relative path in Resources folder</param>
        public static void RemoveFromCache<T>(string path) where T : Object
        {
            Hash128 hash = GetHash<T>(path);
            if (storage.ContainsKey(hash)) storage.Remove(hash);
        }

        /// <summary>
        /// Performs a cache flush. Apply regularly to avoid memory leaks.
        /// </summary>
        public static void ClearCache() => storage.Clear();
    }
}
