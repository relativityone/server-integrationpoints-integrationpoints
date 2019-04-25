using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Internals
{
    internal interface IKeplerRequestHelper
    {
        Task<TResponse> ExecuteWithRetriesAsync<TService, TRequest, TResponse>(
            Func<TService, TRequest, Task<TResponse>> function,
            TRequest request
        ) where TService : IDisposable;
    }
}
