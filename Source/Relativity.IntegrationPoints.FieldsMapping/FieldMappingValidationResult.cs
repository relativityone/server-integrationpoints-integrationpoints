using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldMappingValidationResult
	{
		public List<FieldMap> InvalidMappedFields { get; set; } = new List<FieldMap>();
		public bool IsObjectIdentifierMapValid { get; set; }
	}
}