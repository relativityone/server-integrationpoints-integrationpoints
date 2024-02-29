using System.Runtime.Caching;

namespace Relativity.Sync.Utils
{
    internal class MemoryCacheWrapper : IMemoryCache
    {
        private readonly ObjectCache _memoryCache = MemoryCache.Default;

        public MemoryCacheWrapper()
        {
            // sometimes if the cache is empty, trimming it causes division by zero
            // AddOrGetExisting to take into account that we use a singleton MemoryCache.Default underneath
            // https://developercommunity.visualstudio.com/t/net-452-systemruntimecachingmemorycachestatisticss/539608#T-N603734
            _memoryCache.AddOrGetExisting("NON_REMOVABLE", "NON_REMOVABLE", new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable });
        }

        public T Get<T>(string key)
        {
            return (T)_memoryCache.Get(key);
        }

        public bool Add(string key, object value, CacheItemPolicy policy)
        {
            return _memoryCache.Add(new CacheItem(key, value), policy);
        }

        public T AddOrGetExisting<T>(string key, T value, CacheItemPolicy policy)
        {
            return (T)_memoryCache.AddOrGetExisting(key, value, policy);
        }
    }
}
