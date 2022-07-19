using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.KeplerFactory
{
    internal interface IServiceFactoryFactory
    {
        IServiceFactory Create(ServiceFactorySettings settings);
    }
}