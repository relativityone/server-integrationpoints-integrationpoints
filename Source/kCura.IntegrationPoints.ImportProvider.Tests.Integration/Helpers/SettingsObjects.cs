using System.Collections.Generic;

using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers
{
    public class SettingsObjects
    {
        public ImportSettings ImportSettings { get; set; }

        public ImportProviderSettings ImportProviderSettings { get; set; }

        public List<FieldMap> FieldMaps { get; set; }
    }
}
