using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFederatedInstanceRepository
	{
		FederatedInstanceDto RetrieveFederatedInstance(string name);
		FederatedInstanceDto RetrieveFederatedInstance(int artifactId);
		IEnumerable<FederatedInstanceDto> RetrieveAll();
	}
}