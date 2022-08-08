using System.Collections.Concurrent;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging
{
    internal class CacheHolder : ICacheHolder
    {
        private const string _SESSION_PREFIX = "_RIP_LOG_";

        // As per now Releativity Web guaranties that the user requests will not be redirected to the 
        private static readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public T GetObject<T>(string key) where T : class
        {
            string sessionKey = GetSessionKey(key);
            T item;
            if (_cache.ContainsKey(sessionKey))
            {
                item = (T)_cache[sessionKey];
            }
            else
            {
                item = default(T);
            }
            return item;
        }

        public void SetObject<T>(string key, T value) where T : class
        {
            string sessionKey = GetSessionKey(key);
            _cache.AddOrUpdate(sessionKey, value, (keyParam, existingVal) => value);
        }

        private static string GetSessionKey(string key)
        {
            return _SESSION_PREFIX + key;
        }
    }
}