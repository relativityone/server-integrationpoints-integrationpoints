using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    public class IntegrationPointInfo
    {
        public IntegrationPointInfo(IntegrationPointDto integrationPoint)
        {
            ArtifactId = integrationPoint.ArtifactId;
            SourceConfiguration = integrationPoint.SourceConfiguration;
            DestinationConfiguration = CustomProviderDestinationConfiguration.From(integrationPoint.DestinationConfiguration);
            SecuredConfiguration = integrationPoint.SecuredConfiguration;
            FieldMap = integrationPoint.FieldMappings?.Select((map, i) => new IndexedFieldMap(map, FieldMapType.Normal, i)).ToList();
        }

        public int ArtifactId { get; set; }

        public string SourceConfiguration { get; set; }

        public CustomProviderDestinationConfiguration DestinationConfiguration { get; set; }

        public string SecuredConfiguration { get; set; }

        public List<IndexedFieldMap> FieldMap { get; set; }
    }
}
