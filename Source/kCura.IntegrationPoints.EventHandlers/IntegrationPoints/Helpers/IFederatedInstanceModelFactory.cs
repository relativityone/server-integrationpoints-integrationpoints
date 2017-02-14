using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IFederatedInstanceModelFactory
	{
		FederatedInstanceModel Create(IDictionary<string, object> settings, Artifact artifact);
	}
}