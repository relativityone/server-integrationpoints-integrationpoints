using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain.Managers
{
	public interface IFederatedInstanceManager
	{
		FederatedInstanceDto RetrieveFederatedInstance(int? artifactId);
		IEnumerable<FederatedInstanceDto> RetrieveAll();
	}
}