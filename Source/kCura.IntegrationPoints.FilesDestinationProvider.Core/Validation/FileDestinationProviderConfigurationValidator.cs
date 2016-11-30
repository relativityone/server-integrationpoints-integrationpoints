using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using static kCura.IntegrationPoints.Core.Models.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileDestinationProviderConfigurationValidator : IValidator, IIntegrationPointValidationService
	{
		private readonly ISerializer _serializer;
		private readonly IFileDestinationProviderValidatorsFactory _validatorsFactory;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;

		public FileDestinationProviderConfigurationValidator(
			ISerializer serializer,
			IFileDestinationProviderValidatorsFactory validatorsFactory,
			IExportSettingsBuilder exportSettingsBuilder
		)
		{
			_serializer = serializer;
			_validatorsFactory = validatorsFactory;
			_exportSettingsBuilder = exportSettingsBuilder;
		}

		public string Key => IntegrationPointProviderValidator.GetProviderValidatorKey(
			IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
			IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID
		);

		public ValidationResult Prevalidate(IntegrationPointModel model)
		{
			var result = new ValidationResult();

			var exportFileValidator = _validatorsFactory.CreateExportFileValidator();
			result.Add(exportFileValidator.Validate(model));

			return result;
		}

		public ValidationResult Validate(object value)
		{
			return Validate(value as IntegrationPointProviderValidationModel);
		}

		public ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			var sourceConfiguration = _serializer.Deserialize<ExportUsingSavedSearchSettings>(model.SourceConfiguration);
			var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(model.FieldsMap);

			var exportSettings = _exportSettingsBuilder.Create(sourceConfiguration, fieldMap, model.ArtifactTypeId);

			var workspaceValidator = _validatorsFactory.CreateWorkspaceValidator();
			result.Add(workspaceValidator.Validate(exportSettings.WorkspaceId));

			switch (exportSettings.TypeOfExport)
			{
				case ExportType.Folder:
				case ExportType.FolderAndSubfolders:
					//var folderValidator = _validatorsFactory.CreateFolderValidator();
					//result.Add(folderValidator.Validate(exportSettings));

					var viewValidator = _validatorsFactory.CreateViewValidator();
					result.Add(viewValidator.Validate(exportSettings));
					break;

				case ExportType.ProductionSet:
					var productionsValidator = _validatorsFactory.CreateProductionValidator();
					result.Add(productionsValidator.Validate(exportSettings));
					break;

				case ExportType.SavedSearch:
					var savedSearchValidator = _validatorsFactory.CreateSavedSearchValidator(exportSettings.WorkspaceId, exportSettings.SavedSearchArtifactId);
					result.Add(savedSearchValidator.Validate(exportSettings.SavedSearchArtifactId));
					break;
			}

			// fields / mapping

			// all values match types and ranges

			return result;
		}
	}
}