using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class FieldsMappingValidator : IValidator
	{
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;

		private const string OBJECT_IDENTIFIER_APPENDAGE_TEXT = " [Object Identifier]";

		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public const string ERROR_INTEGRATION_MODEL_VALIDATION_NOT_INITIALIZED = "Integration model validation object not initialized";
		public const string ERROR_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped to Destination: ";
		public const string ERROR_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped to Source: ";
		public const string ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";
		public const string ERROR_IDENTIFIERS_NOT_MATCHED = "Identifier must be mapped with another identifier.";
		public const string ERROR_UNIQUE_IDENTIFIER_MUST_BE_MAPPED = "The unique identifier must be mapped.";
		public const string ERROR_FIELD_MUST_BE_MAPPED = "must be mapped.";
		public const string ERROR_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE = "Field does not exist in source workspace: ";
		public const string ERROR_FIELD_NOT_EXIST_IN_DESTINATION_WORKSPACE = "Field does not exist in destination workspace: ";


		public const string FIELD_NAME = "Name";
		public const string FIELD_IS_IDENTIFIER = "Is Identifier";

		public FieldsMappingValidator(ISerializer serializer, IRepositoryFactory repositoryFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
		}

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();
			var integrationModel = value as IntegrationModelValidation;
			if (integrationModel == null) { throw new Exception(ERROR_INTEGRATION_MODEL_VALIDATION_NOT_INITIALIZED); }
			bool mappedIdentifier = false;

			if (!IsRelativityProvider(integrationModel.SourceProviderId, integrationModel.DestinationProviderId))
			{
				return result;
			}

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
			var fieldsMap = _serializer.Deserialize<List<FieldMap>>(integrationModel.FieldsMap);

			List<ArtifactDTO> sourceWorkpaceFields = RetrieveAllFields(sourceConfiguration.SourceWorkspaceArtifactId);
			List<ArtifactDTO> destinationWorkpaceFields = RetrieveAllFields(sourceConfiguration.TargetWorkspaceArtifactId);

			foreach (FieldMap fieldMap in fieldsMap)
			{
				result.Add(ValidateFieldMapped(fieldMap));
				result.Add(ValidateFieldIdentifierMappedWithAnotherIdentifier(fieldMap));
				result.Add(ValidateFieldExist(fieldMap, sourceWorkpaceFields, destinationWorkpaceFields));

				if (fieldMap.FieldMapType == FieldMapTypeEnum.Identifier)
				{
					mappedIdentifier = true;
				}
			}
			result.Add(ValidateUniqueIdentifierIsMapped(mappedIdentifier));
			result.Add(ValidateAllRequiredFieldsMapped(fieldsMap, destinationWorkpaceFields));

			return result;
		}

		private static bool IsRelativityProvider(string sourceProviderId, string destinationProviderId)
		{
			return string.Equals(sourceProviderId, IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, StringComparison.CurrentCultureIgnoreCase) &&
			       string.Equals(destinationProviderId, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(), StringComparison.CurrentCultureIgnoreCase);
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

			if(fieldMap.SourceField == null || fieldMap.DestinationField == null) { return result; }

			if (fieldMap.SourceField.IsIdentifier &&
			    !fieldMap.DestinationField.IsIdentifier)
			{
				result.Add(ERROR_IDENTIFIERS_NOT_MATCHED);
			}

			return result;
		}

		private ValidationResult ValidateFieldExist(FieldMap fieldMap, List<ArtifactDTO> sourceFields, List<ArtifactDTO> destinationFields)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField?.FieldIdentifier == null || fieldMap.DestinationField?.FieldIdentifier == null) { return result; }
			
			int sourceFieldIdentifier = int.Parse(fieldMap.SourceField.FieldIdentifier);
			int destinationFieldIdentifier = int.Parse(fieldMap.DestinationField.FieldIdentifier);

			if (sourceFields.All(x => x.ArtifactId != sourceFieldIdentifier))
			{
				result.Add(ERROR_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE);
			}
			if (destinationFields.All(x => x.ArtifactId != destinationFieldIdentifier))
			{
				result.Add(ERROR_FIELD_NOT_EXIST_IN_DESTINATION_WORKSPACE);
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

		private ValidationResult ValidateAllRequiredFieldsMapped(List<FieldMap> fieldsMap, List<ArtifactDTO> fieldArtifacts)
		{
			var result = new ValidationResult();
			var requiredFields = new List<string>();
			var missingFields = new List<string>();

			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				string displayName = string.Empty;
				int isIdentifierFieldValue = 0;

				foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
				{
					if (field.Name == FIELD_NAME)
					{
						displayName = field.Value as string;
					}
					else if (field.Name == FIELD_IS_IDENTIFIER)
					{
						isIdentifierFieldValue = Convert.ToInt32(field.Value);
					}
				}

				if (isIdentifierFieldValue > 0)
				{
					displayName += OBJECT_IDENTIFIER_APPENDAGE_TEXT;
					requiredFields.Add(displayName);
				}
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

		private List<ArtifactDTO> RetrieveAllFields(int workspaceId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceId);

			ArtifactDTO[] fieldArtifacts = fieldRepository.RetrieveFields(
				Convert.ToInt32(ArtifactType.Document),
				new HashSet<string>(new[]
				{
					FIELD_NAME,
					FIELD_IS_IDENTIFIER
				}));

			return fieldArtifacts.ToList();
		}
	}
}