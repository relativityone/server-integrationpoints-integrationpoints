using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Kepler
{
    public interface IDynamicProxyFactory
    {
        T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class;
    }
}