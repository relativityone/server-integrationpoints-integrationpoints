using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class IntegrationPointInfo
    {
        public IntegrationPointInfo(IntegrationPointDto integrationPoint)
        {
            SourceConfiguration = integrationPoint.SourceConfiguration;
            SecuredConfiguration = integrationPoint.SecuredConfiguration;
            FieldMap = integrationPoint.FieldMappings?.Select((map, i) => new IndexedFieldMap(map, i)).ToList();
        }

        public string SourceConfiguration { get; set; }

        public string SecuredConfiguration { get; set; }

        public List<IndexedFieldMap> FieldMap { get; set; }

        public int ArtifactTypeId { get; set; }
    }
}
