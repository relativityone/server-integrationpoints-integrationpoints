using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class FederatedInstanceManager : IFederatedInstanceManager
    {
        public static FederatedInstanceDto LocalInstance { get; } = new FederatedInstanceDto
        {
            Name = "This Instance",
            ArtifactId = null
        };

        public FederatedInstanceDto RetrieveFederatedInstanceByArtifactId(int? artifactId)
        {
            return artifactId.HasValue
                ? null // only local instance is supported
                : LocalInstance;
        }

        public IEnumerable<FederatedInstanceDto> RetrieveAll() // only local instance is supported
        {
            yield return LocalInstance;
        }
    }
}
