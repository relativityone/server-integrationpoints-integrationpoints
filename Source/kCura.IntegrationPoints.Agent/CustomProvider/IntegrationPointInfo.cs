using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    public class IntegrationPointInfo
    {
        public IntegrationPointInfo(IntegrationPointDto integrationPoint, int sourceWorkspaceArtifactId)
        {
            ArtifactId = integrationPoint.ArtifactId;
            SourceConfiguration = integrationPoint.SourceConfiguration;
            DestinationConfiguration = integrationPoint.DestinationConfiguration;
            SecuredConfiguration = integrationPoint.SecuredConfiguration;
            FieldMap = integrationPoint.FieldMappings?.Select((map, i) => new IndexedFieldMap(map, FieldMapType.Normal, i)).ToList();
            SourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
        }

        public int ArtifactId { get; }

        public string SourceConfiguration { get; }

        public int SourceWorkspaceArtifactId { get; }

        public DestinationConfiguration DestinationConfiguration { get; set; }

        public string SecuredConfiguration { get; }

        public List<IndexedFieldMap> FieldMap { get; }
    }
}
