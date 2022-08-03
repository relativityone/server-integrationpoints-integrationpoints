using System.Collections.Generic;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import
{
    public class FieldsMappingModel
    {
        public bool MapFieldsAutomatically { get; set; }

        public SortedDictionary<string, string> FieldsMapping { get; set; }

        public FieldsMappingModel(params string[] mappings)
        {
            FieldsMapping = new SortedDictionary<string, string>();

            if (mappings != null)
            {
                for (var i = 0; i < mappings.Length; i += 2)
                {
                    FieldsMapping.Add(mappings[i], mappings[i + 1]);
                }
            }
        }
    }
}