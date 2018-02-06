using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;

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