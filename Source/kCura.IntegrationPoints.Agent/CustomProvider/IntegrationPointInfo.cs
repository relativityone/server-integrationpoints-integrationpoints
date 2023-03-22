using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class IntegrationPointInfo
    {
        public string SourceConfiguration { get; set; }

        public string SecuredConfiguration { get; set; }

        public List<IndexedFieldMap> FieldMap { get; set; }
    }
}
