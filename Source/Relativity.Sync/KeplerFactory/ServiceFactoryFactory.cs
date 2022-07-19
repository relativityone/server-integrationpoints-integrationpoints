using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.KeplerFactory
{
    internal sealed class ServiceFactoryFactory : IServiceFactoryFactory
    {
        public IServiceFactory Create(ServiceFactorySettings settings)
        {
            return new ServiceFactory(settings);
        }
    }
}