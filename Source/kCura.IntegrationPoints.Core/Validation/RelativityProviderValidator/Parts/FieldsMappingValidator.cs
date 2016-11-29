using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class FieldsMappingValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
	{
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;

		private const string _OBJECT_IDENTIFIER_APPENDAGE_TEXT = " [Object Identifier]";

		public FieldsMappingValidator(ISerializer serializer, IRepositoryFactory repositoryFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
		}

		public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
		{
			var result = new ValidationResult();

			if (!IsRelativityProvider(value.SourceProviderIdentifier, value.DestinationProviderIdentifier))
			{
				return result;
			}

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(value.SourceConfiguration);
			var fieldsMap = _serializer.Deserialize<List<FieldMap>>(value.FieldsMap);

			List<ArtifactDTO> sourceWorkpaceFields = RetrieveAllFields(sourceConfiguration.SourceWorkspaceArtifactId);
			List<ArtifactDTO> destinationWorkpaceFields = RetrieveAllFields(sourceConfiguration.TargetWorkspaceArtifactId);

			bool mappedIdentifier = false;

			foreach (FieldMap fieldMap in fieldsMap)
			{
				result.Add(ValidateFieldMapped(fieldMap));
				result.Add(ValidateFieldIdentifierMappedWithAnotherIdentifier(fieldMap));
				result.Add(ValidateFieldExist(fieldMap, sourceWorkpaceFields, destinationWorkpaceFields));

				if ((fieldMap.FieldMapType == FieldMapTypeEnum.Identifier) && 
					(fieldMap.SourceField != null) &&
					(fieldMap.SourceField.IsIdentifier))
				{
					mappedIdentifier = true;
				}
			}

			result.Add(ValidateUniqueIdentifierIsMapped(mappedIdentifier));
			result.Add(ValidateAllRequiredFieldsMapped(fieldsMap, destinationWorkpaceFields));

			return result;
		}

		private bool IsRelativityProvider(string sourceProviderId, string destinationProviderId)
		{
			return string.Equals(sourceProviderId, IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, StringComparison.CurrentCultureIgnoreCase) &&
				   string.Equals(destinationProviderId, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(), StringComparison.CurrentCultureIgnoreCase);
		}

		private ValidationResult ValidateFieldMapped(FieldMap fieldMap)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField == null && fieldMap.DestinationField == null)
			{
				result.Add(RelativityProviderValidationMessages.FIELD_MAP_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED);
			}
			else if (fieldMap.SourceField == null)
			{
				result.Add($"{RelativityProviderValidationMessages.FIELD_MAP_SOURCE_FIELD_NOT_MAPPED} {fieldMap.DestinationField.DisplayName}");
			}
			else if (fieldMap.DestinationField == null)
			{
				result.Add($"{RelativityProviderValidationMessages.FIELD_MAP_DESTINATION_FIELD_NOT_MAPPED} {fieldMap.SourceField.DisplayName}");
			}

			return result;
		}

		private ValidationResult ValidateFieldIdentifierMappedWithAnotherIdentifier(FieldMap fieldMap)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField == null || fieldMap.DestinationField == null)
			{
				return result;
			}

			if (fieldMap.SourceField.IsIdentifier && !fieldMap.DestinationField.IsIdentifier)
			{
				result.Add(RelativityProviderValidationMessages.FIELD_MAP_IDENTIFIERS_NOT_MATCHED);
			}

			return result;
		}

		private ValidationResult ValidateFieldExist(FieldMap fieldMap, List<ArtifactDTO> sourceFields, List<ArtifactDTO> destinationFields)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField?.FieldIdentifier == null || fieldMap.DestinationField?.FieldIdentifier == null)
			{
				return result;
			}

			int sourceFieldIdentifier = int.Parse(fieldMap.SourceField.FieldIdentifier);
			int destinationFieldIdentifier = int.Parse(fieldMap.DestinationField.FieldIdentifier);

			if (sourceFields.All(x => x.ArtifactId != sourceFieldIdentifier))
			{
				result.Add(RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE);
			}

			if (destinationFields.All(x => x.ArtifactId != destinationFieldIdentifier))
			{
				result.Add(RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_DESTINATION_WORKSPACE);
			}

			return result;
		}

		private ValidationResult ValidateUniqueIdentifierIsMapped(bool isMapped)
		{
			var result = new ValidationResult();

			if (!isMapped)
			{
				result.Add(RelativityProviderValidationMessages.FIELD_MAP_UNIQUE_IDENTIFIER_MUST_BE_MAPPED);
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
					if (field.Name == RelativityProviderValidationMessages.FIELD_MAP_FIELD_NAME)
					{
						displayName = field.Value as string;
					}
					else if (field.Name == RelativityProviderValidationMessages.FIELD_MAP_FIELD_IS_IDENTIFIER)
					{
						isIdentifierFieldValue = Convert.ToInt32(field.Value);
					}
				}

				if (isIdentifierFieldValue > 0)
				{
					displayName += _OBJECT_IDENTIFIER_APPENDAGE_TEXT;
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
				result.Add($"The {fieldMessage} {fieldPlural} {RelativityProviderValidationMessages.FIELD_MAP_FIELD_MUST_BE_MAPPED}");
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
					RelativityProviderValidationMessages.FIELD_MAP_FIELD_NAME,
					RelativityProviderValidationMessages.FIELD_MAP_FIELD_IS_IDENTIFIER
				}));

			return fieldArtifacts.ToList();
		}
	}
}