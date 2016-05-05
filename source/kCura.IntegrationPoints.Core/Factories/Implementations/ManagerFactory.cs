using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ManagerFactory : IManagerFactory
	{
		public IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer)
		{
			return new IntegrationPointManager(contextContainer);
		}

		public ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer)
		{
			return new SourceProviderManager(contextContainer);
		}
	}
}
