using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class FieldMappingValidationResult
    {
        public IList<InvalidFieldMap> InvalidMappedFields { get; set; } = new List<InvalidFieldMap>();

        public bool IsObjectIdentifierMapValid { get; set; }
    }
}
