using System.Runtime.Caching;

namespace Relativity.Sync.Utils
{
	internal class MemoryCacheWrapper : IMemoryCache
	{
		private readonly ObjectCache _memoryCache = MemoryCache.Default;

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
