using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class IntegrationPointManagerFactory : IIntegrationPointManagerFactory
	{
		public IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer)
		{
			return new IntegrationPointManager(contextContainer);
		}
	}
}
