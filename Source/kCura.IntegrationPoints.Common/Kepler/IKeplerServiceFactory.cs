using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Kepler
{
    public interface IKeplerServiceFactory
    {
        Task<T> CreateProxyAsync<T>() where T : class, IDisposable;
    }
}