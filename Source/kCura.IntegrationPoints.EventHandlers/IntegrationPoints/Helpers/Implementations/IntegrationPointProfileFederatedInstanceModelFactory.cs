using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointProfileFederatedInstanceModelFactory : IFederatedInstanceModelFactory
	{
		public FederatedInstanceModel Create(IDictionary<string, object> settings, Artifact artifact)
		{
			return new FederatedInstanceModel();
		}
	}
}