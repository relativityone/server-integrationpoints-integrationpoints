using System.Runtime.Caching;

namespace Relativity.Sync.Utils
{
    internal interface IMemoryCache
    {
        T Get<T>(string key);

        bool Add(string key, object value, CacheItemPolicy policy);

        T AddOrGetExisting<T>(string key, T value, CacheItemPolicy policy);
    }
}
