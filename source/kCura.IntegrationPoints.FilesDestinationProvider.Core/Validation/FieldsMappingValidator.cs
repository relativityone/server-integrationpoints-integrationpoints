using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FieldsMappingValidator : IValidator
	{
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();
			var fieldMapValidation = value as IntegrationModelValidation;

			if (fieldMapValidation != null &&
			    fieldMapValidation.SourceProviderId == Domain.Constants.RELATIVITY_PROVIDER_GUID &&
			    fieldMapValidation.DestinationProviderId == Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID)
			{
				foreach (FieldMap fieldMap in fieldMapValidation.FieldsMap)
				{
					//TODO ...
				}
			}

			return result;
		}
	}
}