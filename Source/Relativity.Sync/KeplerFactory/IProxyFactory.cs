using System;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    internal interface IProxyFactory
    {
        Task<T> CreateProxyAsync<T>() where T : class, IDisposable;
    }
}
