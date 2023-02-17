using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain.Managers
{
    public interface IFederatedInstanceManager
    {
        FederatedInstanceDto RetrieveFederatedInstanceByArtifactId(int? artifactId);
        IEnumerable<FederatedInstanceDto> RetrieveAll();
    }
}
