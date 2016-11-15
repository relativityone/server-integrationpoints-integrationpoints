using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class FieldMappingsValidator : IProviderValidator
	{
		private readonly string _fieldsMappings;

		public FieldMappingsValidator(string fieldMappings)
		{
			_fieldsMappings = fieldMappings;
		}

		public ValidationResult Validate()
		{
			throw new System.NotImplementedException();
		}
	}
}
