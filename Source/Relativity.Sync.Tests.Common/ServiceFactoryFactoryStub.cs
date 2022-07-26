using System.Diagnostics.CodeAnalysis;
using Moq;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    internal sealed class ServiceFactoryFactoryStub : IServiceFactoryFactory
    {
        public ServiceFactorySettings Settings { get; private set; }

        public IServiceFactory Create(ServiceFactorySettings settings)
        {
            Settings = settings;
            return Mock.Of<IServiceFactory>();
        }
    }
}