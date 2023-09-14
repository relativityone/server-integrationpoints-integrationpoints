using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1.Builders.Documents;
using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow
{
    /// <inheritdoc />
    internal class DocumentImportSettingsBuilder : IDocumentImportSettingsBuilder
    {
        private readonly IInstanceSettings _instanceSettings;
        private readonly IAPILog _logger;

        public DocumentImportSettingsBuilder(
            IInstanceSettings instanceSettings,
            IAPILog logger)
        {
            _instanceSettings = instanceSettings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<DocumentImportConfiguration> BuildAsync(CustomProviderDestinationConfiguration destinationConfiguration, List<IndexedFieldMap> fieldMappings, IndexedFieldMap identifierField)
        {
            IWithOverlayMode overlayModeSettings = ImportDocumentSettingsBuilder.Create();

            AdvancedImportSettings advancedSettings = await CreateAdvancedImportSettingsAsync();

            IWithNatives nativesSettings = ConfigureOverwriteModeSettings(
                overlayModeSettings,
                destinationConfiguration.ImportOverwriteMode,
                destinationConfiguration.FieldOverlayBehavior,
                identifierField.DestinationFieldName);

            IWithFieldsMapping fieldsMappingSettings = ConfigureFileImportSettings(nativesSettings, destinationConfiguration.ImportNativeFileCopyMode);

            IWithFolders withFolders = ConfigureFieldsMappingSettings(
                fieldsMappingSettings,
                fieldMappings);

            ImportDocumentSettings importDocumentSettings = ConfigureDestinationFolderStructure(
                withFolders,
                destinationConfiguration.DestinationFolderArtifactId,
                destinationConfiguration.UseFolderPathInformation,
                destinationConfiguration.FolderPathSourceField);

            ConfigureMoveExistingDocuments(
                advancedSettings,
                destinationConfiguration.MoveExistingDocuments,
                destinationConfiguration.ImportOverwriteMode,
                destinationConfiguration.FolderPathSourceField);

            return new DocumentImportConfiguration(importDocumentSettings, advancedSettings);
        }

        private void ConfigureMoveExistingDocuments(
            AdvancedImportSettings advancedSettings,
            bool moveExistingDocuments,
            ImportOverwriteModeEnum importOverwriteMode,
            int folderPathFieldIndex)
        {
            advancedSettings.Folder.MoveExistingDocuments =
                importOverwriteMode != ImportOverwriteModeEnum.AppendOnly &&
                moveExistingDocuments &&
                folderPathFieldIndex >= 0;
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

        private IWithFolders ConfigureFieldsMappingSettings(IWithFieldsMapping fieldsMappingSettings, List<IndexedFieldMap> fieldMappings)
        {
            _logger.LogInformation("Configuring FieldsMapping...");
            return fieldsMappingSettings.WithFieldsMapped(x =>
            {
                foreach (IndexedFieldMap map in fieldMappings)
                {
                    _logger.LogInformation("Configure Field - {@field}", map);

                    switch (map.FieldMap.DestinationField.Type)
                    {
                        case FieldTypes.LongText:
                            x = x.WithLongTextFieldContainingFilePath(map.ColumnIndex, map.DestinationFieldName, encoding => encoding
                                .WithEncodingAutoDetection()
                                .WithoutFileSize());
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
            int destinationFolderArtifactId,
            bool useFolderPathInformation,
            int folderPathFieldIndex)
        {
            if (useFolderPathInformation)
            {
                return withFolders.WithFolders(f =>
                    f.WithRootFolderID(destinationFolderArtifactId, r =>
                        r.WithFolderPathDefinedInColumn(folderPathFieldIndex)));
            }
            else
            {
                return withFolders.WithFolders(f =>
                    f.WithRootFolderID(destinationFolderArtifactId, r =>
                        r.WithAllDocumentsInRootFolder()));
            }
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
