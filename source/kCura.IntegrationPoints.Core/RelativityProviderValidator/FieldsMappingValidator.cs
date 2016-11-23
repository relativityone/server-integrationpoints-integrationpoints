using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.RelativityProviderValidator
{
	public class FieldsMappingValidator : IValidator
	{
		private readonly ISerializer _serializer;
		private readonly IFieldProvider _rdoFieldSynchronizerBase;
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public const string ERROR_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped to Destination: ";
		public const string ERROR_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped to Source: ";
		public const string ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";
		public const string ERROR_IDENTIFIERS_NOT_MATCHED = "Identifier must be mapped with another identifier.";
		public const string ERROR_UNIQUE_IDENTIFIER_MUST_BE_MAPPED = "The unique identifier must be mapped.";
		public const string ERROR_FIELD_MUST_BE_MAPPED = "must be mapped.";

		public FieldsMappingValidator(ISerializer serializer, IFieldProvider rdoFieldSynchronizerBase)
		{
			_serializer = serializer;
			_rdoFieldSynchronizerBase = rdoFieldSynchronizerBase;
		}

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();
			var integrationModel = value as IntegrationModelValidation;
			bool mappedIdentifier = false;

			if (integrationModel == null){return result;}

			if (integrationModel.SourceProviderId != IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID ||
			    integrationModel.DestinationProviderId != Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString())
			{
				return result;
			}

			var fieldsMap = _serializer.Deserialize<IEnumerable<FieldMap>>(integrationModel.FieldsMap);
			
			foreach (FieldMap fieldMap in fieldsMap)
			{
				result.Add(ValidateFieldMapped(fieldMap));
				result.Add(ValidateFieldIdentifierMappedWithAnotherIdentifier(fieldMap));

				if (fieldMap.FieldMapType == FieldMapTypeEnum.Identifier)
				{
					mappedIdentifier = true;
				}
			}
			result.Add(ValidateUniqueIdentifierIsMapped(mappedIdentifier));

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

		private ValidationResult ValidateAllRequiredFieldsMapped(List<FieldMap> fieldsMap, string destinationConfiguration)
		{
			var result = new ValidationResult();
			var requiredFields = new List<string>();
			var missingFields = new List<string>();
			IEnumerable<FieldEntry> fields = _rdoFieldSynchronizerBase.GetFields(destinationConfiguration);

			foreach (FieldEntry field in fields)
			{
				if(field.IsRequired) { requiredFields.Add(field.DisplayName);}
			}

			foreach (string requiredField in requiredFields)
			{
				if (fieldsMap.All(x => x.SourceField.DisplayName != requiredField))
				{
					missingFields.Add(requiredField);
				}
			}
			if (missingFields.Count > 0)
			{
				var fieldMessage = string.Join(" and ", missingFields);
				string fieldPlural = requiredFields.Count == 1 ? "field" : "fields";
				result.Add($"The {fieldMessage} {fieldPlural} {ERROR_FIELD_MUST_BE_MAPPED}");
			}

			return result;
		}
	}
}