using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.RelativitySync.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1.Builders.Documents;
using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc />
    internal class DocumentImportSettingsBuilder : IDocumentImportSettingsBuilder
    {
        private readonly IInstanceSettings _instanceSettings;
        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        public DocumentImportSettingsBuilder(
            IInstanceSettings instanceSettings,
            ISerializer serializer,
            IAPILog logger)
        {
            _instanceSettings = instanceSettings;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<DocumentImportConfiguration> BuildAsync(string destinationConfiguration, List<FieldMapWrapper> fieldMappings)
        {
            var configuration = _serializer.Deserialize<ImportSettings>(destinationConfiguration);
            var folderConf = _serializer.Deserialize<FolderConf>(destinationConfiguration);

            IWithOverlayMode overlayModeSettings = global::Relativity.Import.V1.Builders.Documents.ImportDocumentSettingsBuilder.Create();

            AdvancedImportSettings advancedSettings = await CreateAdvancedImportSettingsAsync();

            FieldMapWrapper identifier = GetIdentifierField(fieldMappings);
            IWithNatives nativesSettings = ConfigureOverwriteModeSettings(
                overlayModeSettings,
                configuration.ImportOverwriteMode,
                configuration.FieldOverlayBehavior,
                identifier.DestinationFieldName);

            IWithFieldsMapping fieldsMappingSettings = ConfigureFileImportSettings(nativesSettings, configuration.ImportNativeFileCopyMode);

            IWithFolders withFolders = ConfigureFieldsMappingSettings(
                fieldsMappingSettings,
                fieldMappings);

            ImportDocumentSettings importSettings = ConfigureDestinationFolderStructure(
                withFolders,
                fieldMappings,
                configuration.DestinationFolderArtifactId,
                folderConf.UseFolderPathInformation,
                configuration.FolderPathSourceFieldName);

            ConfigureMoveExistingDocuments(
                advancedSettings,
                configuration.MoveExistingDocuments,
                configuration.ImportOverwriteMode,
                configuration.FolderPathSourceFieldName);

            return new DocumentImportConfiguration(importSettings, advancedSettings);
        }

        private void ConfigureMoveExistingDocuments(
            AdvancedImportSettings advancedSettings,
            bool moveExistingDocuments,
            ImportOverwriteModeEnum importOverwriteMode,
            string folderPathField)
        {
            advancedSettings.Folder.MoveExistingDocuments =
                importOverwriteMode != ImportOverwriteModeEnum.AppendOnly &&
                moveExistingDocuments &&
                !string.IsNullOrEmpty(folderPathField);
        }

        private async Task<AdvancedImportSettings> CreateAdvancedImportSettingsAsync()
        {
            var advancedSettings = new AdvancedImportSettings()
            {
                Folder = new AdvancedFolderSettings(),
                Other = new AdvancedOtherSettings()
            };

            advancedSettings.Other.AuditLevel = AuditLevel.FullAudit;
            advancedSettings.Other.BatchSize = await _instanceSettings.GetCustomProviderBatchSizeAsync().ConfigureAwait(false);

            return advancedSettings;
        }

        private IWithNatives ConfigureOverwriteModeSettings(
            IWithOverlayMode overlayModeSettings,
            ImportOverwriteModeEnum overwriteMode,
            string overlayBehavior,
            string identifierFieldName)
        {
            _logger.LogInformation(
                "Configuring OverlayMode - OverwriteMode: {overwriteMode}, OverlayBehavior: {overlayBehavior}", overwriteMode, overlayBehavior);
            switch (overwriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly:
                    return overlayModeSettings.WithAppendMode();

                case ImportOverwriteModeEnum.AppendOverlay:
                    return overlayModeSettings.WithAppendOverlayMode(
                        x => x.WithKeyField(identifierFieldName)
                            .WithMultiFieldOverlayBehaviour(ToMultiFieldOverlayBehaviour(overlayBehavior)));
                case ImportOverwriteModeEnum.OverlayOnly:
                    return overlayModeSettings.WithOverlayMode(
                        x => x.WithKeyField(identifierFieldName)
                            .WithMultiFieldOverlayBehaviour(ToMultiFieldOverlayBehaviour(overlayBehavior)));
                default:
                    throw new NotSupportedException($"ImportOverwriteMode {overwriteMode} is not supported.");
            }
        }

        private IWithFieldsMapping ConfigureFileImportSettings(IWithNatives nativesSettings, ImportNativeFileCopyModeEnum nativeFileCopyMode)
        {
            _logger.LogInformation("Configuring FileImport - NativeFileCopyMode: {nativeFileCopyMode}", nativeFileCopyMode);

            if (nativeFileCopyMode == ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
            {
                return nativesSettings
                    .WithoutNatives()
                    .WithoutImages();
            }
            else
            {
                throw new NotSupportedException("Native File Import is not supported.");
            }
        }

        private IWithFolders ConfigureFieldsMappingSettings(IWithFieldsMapping fieldsMappingSettings, List<FieldMapWrapper> fieldMappings)
        {
            _logger.LogInformation("Configuring FieldsMapping...");
            return fieldsMappingSettings.WithFieldsMapped(x =>
            {
                foreach (FieldMapWrapper map in fieldMappings)
                {
                    _logger.LogInformation("Configure Field - {@field}", map);

                    switch (map.FieldMap.DestinationField.Type)
                    {
                        case FieldTypes.LongText:
                            x = x.WithLongTextFieldContainingFilePath(map.ColumnIndex, map.DestinationFieldName);
                            break;
                        default:
                            x = x.WithField(map.ColumnIndex, map.DestinationFieldName);
                            break;
                    }
                }
            });
        }

        private ImportDocumentSettings ConfigureDestinationFolderStructure(
            IWithFolders withFolders,
            List<FieldMapWrapper> fieldMappings,
            int destinationFolderArtifactId,
            bool useFolderPathInformation,
            string folderPathField)
        {
            if (useFolderPathInformation)
            {
                return withFolders.WithFolders(f =>
                    f.WithRootFolderID(destinationFolderArtifactId, r =>
                        r.WithAllDocumentsInRootFolder()));
            }
            else
            {
                return withFolders.WithFolders(f =>
                    f.WithRootFolderID(destinationFolderArtifactId, r =>
                        r.WithFolderPathDefinedInColumn(
                            GetFieldIndex(fieldMappings, folderPathField))));
            }
        }

        private static int GetFieldIndex(List<FieldMapWrapper> fieldMappings, string fieldName)
        {
            FieldMapWrapper field = fieldMappings.FirstOrDefault(x => x.DestinationFieldName == fieldName);
            return field?.ColumnIndex ?? -1;
        }

        private static FieldMapWrapper GetIdentifierField(List<FieldMapWrapper> fieldMappings)
        {
            return fieldMappings.FirstOrDefault(x => x.FieldMap.DestinationField.IsIdentifier);
        }

        private static MultiFieldOverlayBehaviour ToMultiFieldOverlayBehaviour(string overlayBehaviorString)
        {
            switch (overlayBehaviorString)
            {
                case ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE: return MultiFieldOverlayBehaviour.MergeAll;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_REPLACE: return MultiFieldOverlayBehaviour.ReplaceAll;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_DEFAULT: return MultiFieldOverlayBehaviour.UseRelativityDefaults;
                default: throw new NotSupportedException($"Unknown {nameof(overlayBehaviorString)} value: {overlayBehaviorString}");
            }
        }
    }
}
