using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using Relativity;
using Relativity.IntegrationPoints.FieldsMapping.Models;

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
            Domain.Constants.RELATIVITY_PROVIDER_GUID,
            IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID
        );

        public ValidationResult Prevalidate(IntegrationPointProviderValidationModel model)
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
            ExportUsingSavedSearchSettings sourceConfiguration = _serializer.Deserialize<ExportUsingSavedSearchSettings>(model.SourceConfiguration);
            IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(model.FieldsMap);

            var exportSettings = _exportSettingsBuilder.Create(sourceConfiguration, fieldMap, model.ArtifactTypeId);

            var workspaceValidator = _validatorsFactory.CreateWorkspaceValidator();
            result.Add(workspaceValidator.Validate(exportSettings.WorkspaceId));

            var fieldsMapValidator = _validatorsFactory.CreateFieldsMapValidator();
            result.Add(fieldsMapValidator.Validate(model));

            var settingsValidator = _validatorsFactory.CreateExportSettingsValidator(model.ArtifactTypeId);
            result.Add(settingsValidator.Validate(exportSettings));

            switch (exportSettings.TypeOfExport)
            {
                case ExportSettings.ExportType.Folder:
                case ExportSettings.ExportType.FolderAndSubfolders:
                    if (model.ArtifactTypeId == (int)ArtifactType.Document)
                    {
                        var folderValidator = _validatorsFactory.CreateArtifactValidator(exportSettings.WorkspaceId, "Folder");
                        result.Add(folderValidator.Validate(exportSettings.FolderArtifactId));
                    }

                    var viewValidator = _validatorsFactory.CreateViewValidator();
                    result.Add(viewValidator.Validate(exportSettings));

                    var exportNativeSettingsValidator = _validatorsFactory.CreateExportNativeSettingsValidator();
                    result.Add(exportNativeSettingsValidator.Validate(model));
                    break;

                case ExportSettings.ExportType.ProductionSet:
                    var productionsValidator = _validatorsFactory.CreateExportProductionValidator();
                    result.Add(productionsValidator.Validate(exportSettings));
                    break;

                case ExportSettings.ExportType.SavedSearch:
                    var savedSearchValidator = _validatorsFactory.CreateSavedSearchValidator(exportSettings.WorkspaceId, exportSettings.SavedSearchArtifactId);
                    result.Add(savedSearchValidator.Validate(exportSettings.SavedSearchArtifactId));
                    break;
            }

            return result;
        }
    }
}