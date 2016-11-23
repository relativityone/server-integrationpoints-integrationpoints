using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.RelativityProviderValidator
{
	public class FieldsMappingValidator : IValidator
	{
		private readonly ISerializer _serializer;
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public const string ERROR_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped to Destination: ";
		public const string ERROR_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped to Source: ";
		public const string ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";
		public const string ERROR_IDENTIFIERS_NOT_MATCHED = "Identifier must be mapped with another identifier.";
		public const string ERROR_UNIQUE_IDENTIFIER_MUST_BE_MAPPED = "The unique identifier must be mapped.";

		public FieldsMappingValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();
			var fieldMapValidation = value as IntegrationModelValidation;
			bool mappedIdentifier = false;

			//TODO var fieldMaps = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldMappings);

			if (fieldMapValidation != null &&
			    fieldMapValidation.SourceProviderId == IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID &&
			    fieldMapValidation.DestinationProviderId == Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString())
			{

				foreach (FieldMap fieldMap in fieldMapValidation.FieldsMap)
				{
					result.Add(ValidateFieldMapped(fieldMap));
					result.Add(ValidateFieldIdentifierMappedWithAnotherIdentifier(fieldMap));

					if (fieldMap.FieldMapType == FieldMapTypeEnum.Identifier)
					{
						mappedIdentifier = true;
					}
				}
				result.Add(ValidateUniqueIdentifierIsMapped(mappedIdentifier));
			}

			return result;
		}

		private static ValidationResult ValidateFieldMapped(FieldMap fieldMap)
		{
			var result = new ValidationResult();
			
			if (fieldMap.SourceField == null && fieldMap.DestinationField == null)
			{
				result.Add(ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED);
			}
			else if (fieldMap.SourceField == null)
			{
				result.Add($"{ERROR_SOURCE_FIELD_NOT_MAPPED} {fieldMap.DestinationField.DisplayName}");
			}
			else if (fieldMap.DestinationField == null)
			{
				result.Add($"{ERROR_DESTINATION_FIELD_NOT_MAPPED} {fieldMap.SourceField.DisplayName}");
			}

			return result;
		}

		private ValidationResult ValidateFieldIdentifierMappedWithAnotherIdentifier(FieldMap fieldMap)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField.IsIdentifier &&
			    !fieldMap.DestinationField.IsIdentifier)
			{
				result.Add(ERROR_IDENTIFIERS_NOT_MATCHED);
			}

			return result;
		}

		private ValidationResult ValidateUniqueIdentifierIsMapped(bool isMapped)
		{
			var result = new ValidationResult();
			if (!isMapped)
			{
				result.Add(ERROR_UNIQUE_IDENTIFIER_MUST_BE_MAPPED);
			}
			return result;
		}
	}
}