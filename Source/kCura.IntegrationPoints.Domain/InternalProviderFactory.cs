using System;
using Castle.Windsor;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain
{
    public class InternalProviderFactory : IProviderFactory
    {
        private readonly IWindsorContainer _container;

        public InternalProviderFactory(IWindsorContainer container)
        {
            _container = container;
        }

        public IDataSourceProvider CreateProvider(Guid identifier)
        {
            return _container.Resolve<IDataSourceProvider>(identifier.ToString());
        }
    }
}
