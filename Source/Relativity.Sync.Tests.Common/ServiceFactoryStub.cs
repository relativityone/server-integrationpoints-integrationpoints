using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    internal sealed class ServiceFactoryStub : ISourceServiceFactoryForAdmin, ISourceServiceFactoryForUser, IDestinationServiceFactoryForAdmin, IDestinationServiceFactoryForUser
    {
        private readonly ServiceFactory _serviceFactory;

        public ServiceFactoryStub(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {
            await Task.Yield();

            return _serviceFactory.CreateProxy<T>();
        }
    }
}
