using System;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    internal interface IDynamicProxyFactory
    {
        T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class;
    }
}
