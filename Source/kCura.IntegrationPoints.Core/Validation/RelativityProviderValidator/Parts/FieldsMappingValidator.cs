﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class FieldsMappingValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
	{
		private readonly IAPILog _logger;
		private readonly ISerializer _serializer;
		private readonly IFieldManager _sourcefieldManager;
		private readonly IFieldManager _targetfieldManager;

		public FieldsMappingValidator(IAPILog logger, ISerializer serializer, IFieldManager sourcefieldManager, IFieldManager targetfieldManager)
		{
			_logger = logger.ForContext<FieldsMappingValidator>();
			_serializer = serializer;
			_sourcefieldManager = sourcefieldManager;
			_targetfieldManager = targetfieldManager;
		}

		public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
		{
			var result = new ValidationResult();

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(value.SourceConfiguration);
			var destinationConfiguration = _serializer.Deserialize<IntegrationPointDestinationConfiguration>(value.DestinationConfiguration);
			var fieldsMap = _serializer.Deserialize<List<FieldMap>>(value.FieldsMap);

			List<ArtifactDTO> sourceWorkpaceFields = RetrieveAllFields(_sourcefieldManager, sourceConfiguration.SourceWorkspaceArtifactId);
			List<ArtifactDTO> destinationWorkpaceFields = RetrieveAllFields(_targetfieldManager, sourceConfiguration.TargetWorkspaceArtifactId);

			bool mappedIdentifier = false;

			foreach (FieldMap fieldMap in fieldsMap)
			{
				result.Add(ValidateFieldMapped(fieldMap));
				result.Add(ValidateFieldIdentifierMappedWithAnotherIdentifier(fieldMap));
				result.Add(ValidateMappedFieldExist(fieldMap, sourceWorkpaceFields, destinationWorkpaceFields));

				if ((fieldMap.FieldMapType == FieldMapTypeEnum.Identifier) && 
					(fieldMap.SourceField != null) &&
					(fieldMap.SourceField.IsIdentifier))
				{
					mappedIdentifier = true;
				}
			}

			result.Add(ValidateUseDynamicFolderPathSettings(destinationConfiguration));
			result.Add(ValidateUniqueIdentifierIsMapped(mappedIdentifier));
			result.Add(ValidateSettingsFieldOverlayBehavior(destinationConfiguration));
			result.Add(ValidateSettingsFolderPathInformation(sourceWorkpaceFields, destinationConfiguration));
			result.Add(ValidateImageSettings(destinationConfiguration, fieldsMap));
			return result;
		}

		private class IntegrationPointDestinationConfiguration
		{
			public int FolderPathSourceField { get; set; }
			public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }
			public bool UseFolderPathInformation { get; set; }
			public string FieldOverlayBehavior { get; set; }
			public bool UseDynamicFolderPath { get; set; }
			public bool ImageImport { get; set; }
		}

		#region Internal validators

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

		private ValidationResult ValidateMappedFieldExist(FieldMap fieldMap, List<ArtifactDTO> sourceFields, List<ArtifactDTO> destinationFields)
		{
			var result = new ValidationResult();

			if (fieldMap.SourceField?.FieldIdentifier == null || fieldMap.DestinationField?.FieldIdentifier == null)
			{
				return result;
			}

			int sourceFieldIdentifier = int.Parse(fieldMap.SourceField.FieldIdentifier);
			int destinationFieldIdentifier = int.Parse(fieldMap.DestinationField.FieldIdentifier);
			
			result.Add(ValidateFieldExists(sourceFieldIdentifier, sourceFields, RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE));
			result.Add(ValidateFieldExists(destinationFieldIdentifier, destinationFields, RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_DESTINATION_WORKSPACE));
			
			return result;
		}

		private ValidationResult ValidateFieldExists(int fieldArtifactId, List<ArtifactDTO> fields, string validationMessage)
		{
			var result = new ValidationResult();

			if (fields.All(x => x.ArtifactId != fieldArtifactId))
			{
				result.Add($"{validationMessage} {fieldArtifactId}");
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

		private ValidationResult ValidateSettingsFieldOverlayBehavior(IntegrationPointDestinationConfiguration destinationConfig)
		{
			var result = new ValidationResult();
			
			if (destinationConfig.ImportOverwriteMode == ImportOverwriteModeEnum.AppendOnly)
			{
				if (destinationConfig.FieldOverlayBehavior !=
				    RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT)
				{
					result.Add(RelativityProviderValidationMessages.FIELD_MAP_APPEND_ONLY_INVALID_OVERLAY_BEHAVIOR);
				}
			}
			else
			{
				if (destinationConfig.FieldOverlayBehavior !=
				    RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE &&
				    destinationConfig.FieldOverlayBehavior !=
				    RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_REPLACE &&
				    destinationConfig.FieldOverlayBehavior !=
				    RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT)
				{
					result.Add($"{RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_INVALID}{destinationConfig.FieldOverlayBehavior}");
				}
			}

			return result;
		}

		private ValidationResult ValidateSettingsFolderPathInformation(List<ArtifactDTO> sourceWorkpaceFields, IntegrationPointDestinationConfiguration destinationConfig)
		{
			var result = new ValidationResult();

			if (destinationConfig.ImportOverwriteMode == ImportOverwriteModeEnum.AppendOnly ||
				destinationConfig.ImportOverwriteMode == ImportOverwriteModeEnum.OverlayOnly ||
			    destinationConfig.ImportOverwriteMode == ImportOverwriteModeEnum.AppendOverlay)
			{
				if (destinationConfig.UseFolderPathInformation)
				{
					result.Add(ValidateFieldExists(destinationConfig.FolderPathSourceField, sourceWorkpaceFields,
						RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE));
				}
			}

			return result;
		}

		private ValidationResult ValidateUseDynamicFolderPathSettings(IntegrationPointDestinationConfiguration destinationConfig)
		{
			var result = new ValidationResult();

			if (destinationConfig.UseDynamicFolderPath && destinationConfig.UseFolderPathInformation)
			{
				result.Add(new ValidationResult(false, RelativityProviderValidationMessages.FIELD_MAP_DYNAMIC_FOLDER_PATH_AND_FOLDER_PATH_INFORMATION_CONFLICT));
			}

			return result;
		}

		private ValidationResult ValidateImageSettings(IntegrationPointDestinationConfiguration destinationConfiguration, List<FieldMap> mappedFields)
		{
			var result = new ValidationResult();

			if (destinationConfiguration.ImageImport)
			{
				if (mappedFields.Count > 1)
				{
					result.Add(RelativityProviderValidationMessages.FIELD_MAP_IMAGE_TOO_MANY_FIELDS);
				}
			}
			return result;
		}
		#endregion

		private List<ArtifactDTO> RetrieveAllFields(IFieldManager fieldManager, int workspaceId)
		{
			try
			{
				ArtifactDTO[] fieldArtifacts = fieldManager.RetrieveFields(workspaceId,
					new HashSet<string>(new[]
					{
						RelativityProviderValidationMessages.FIELD_MAP_FIELD_NAME,
						RelativityProviderValidationMessages.FIELD_MAP_FIELD_IS_IDENTIFIER
					})
				);

				return fieldArtifacts.ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred retrieving fields in {validator}", nameof(FieldsMappingValidator));
				string message =
					IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("retrieving fields");
				throw new IntegrationPointsException(message, ex);
			}
		}
	}
}