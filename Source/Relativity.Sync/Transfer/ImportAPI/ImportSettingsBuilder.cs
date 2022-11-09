using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Builders.Documents;
using Relativity.Import.V1.Models.Settings;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal class ImportSettingsBuilder : IImportSettingsBuilder
    {
        private readonly IFieldManager _fieldManager;
        private readonly ISyncToggles _syncToggles;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IAPILog _logger;

        public ImportSettingsBuilder(
            IFieldManager fieldManager,
            ISyncToggles syncToggles,
            IInstanceSettings instanceSettings,
            IAPILog logger)
        {
            _fieldManager = fieldManager;
            _syncToggles = syncToggles;
            _instanceSettings = instanceSettings;
            _logger = logger;
        }

        public async Task<(ImportDocumentSettings importSettings, AdvancedImportSettings advancedSettings)> BuildAsync(IConfigureDocumentSynchronizationConfiguration configuration, CancellationToken token)
        {
            _logger.LogInformation("Creating ImportDocumentSettingsBuilder...");

            IReadOnlyList<FieldInfoDto> mappedFields = await RetrieveMappedFieldsAsync(configuration.ImageImport, token).ConfigureAwait(false);

            _logger.LogInformation("FieldsMapping retrieved - Fields Count: {mappingsCount}", mappedFields.Count);

            IWithOverlayMode overlayModeSettings = ImportDocumentSettingsBuilder.Create();

            AdvancedImportSettings advancedSettings = CreateAdvancedImportSettings();

            IWithNatives nativesSettings = ConfigureOverwriteModeSettings(
                overlayModeSettings,
                configuration.ImportOverwriteMode,
                configuration.FieldOverlayBehavior,
                mappedFields);

            IWithFieldsMapping fieldsMappingSettings = ConfigureFileImportSettings(
                nativesSettings,
                advancedSettings,
                configuration.ImageImport,
                configuration.ImportNativeFileCopyMode,
                mappedFields);

            IWithFolders withFolders = ConfigureFieldsMappingSettings(
                fieldsMappingSettings,
                mappedFields);

            ImportDocumentSettings importSettings = ConfigureDestinationFolderStructure(
                withFolders,
                mappedFields,
                configuration.DestinationFolderStructureBehavior,
                configuration.DataDestinationArtifactId,
                configuration.FolderPathField);

            ConfigureAuditSettings(advancedSettings);

            await ConfigureBatchSizeSettingsAsync(advancedSettings).ConfigureAwait(false);

            return (importSettings, advancedSettings);
        }

        private AdvancedImportSettings CreateAdvancedImportSettings()
        {
            return new AdvancedImportSettings()
            {
                Native = new AdvancedNativeSettings
                {
                    FileType = new FileTypeSettings()
                },
                Folder = new AdvancedFolderSettings(),
                Other = new AdvancedOtherSettings()
            };
        }

        private async Task ConfigureBatchSizeSettingsAsync(AdvancedImportSettings advancedSettings)
        {
            advancedSettings.Other.BatchSize = await _instanceSettings.GetImportApiBatchSizeAsync().ConfigureAwait(false);
        }

        private void ConfigureAuditSettings(AdvancedImportSettings advancedSettings)
        {
            advancedSettings.Other.AuditLevel = _syncToggles.IsEnabled<EnableAuditToggle>()
                ? AuditLevel.FullAudit
                : AuditLevel.NoAudit;

            _logger.LogInformation("ImportJob was configured to run with {audit} mode.", advancedSettings.Other.AuditLevel);
        }

        private IWithNatives ConfigureOverwriteModeSettings(
            IWithOverlayMode overlayModeSettings,
            ImportOverwriteMode overwriteMode,
            FieldOverlayBehavior overlayBehavior,
            IEnumerable<FieldInfoDto> mappedFields)
        {
            string identityKeyField = mappedFields.Single(x => x.IsIdentifier).DestinationFieldName;

            _logger.LogInformation(
                "Configuring OverlayMode - OverwriteMode: {overwriteMode}, OverlayBehavior: {overlayBehavior}", overwriteMode, overlayBehavior);
            switch (overwriteMode)
            {
                case ImportOverwriteMode.AppendOnly:
                    return overlayModeSettings.WithAppendMode();

                case ImportOverwriteMode.AppendOverlay:
                    return overlayModeSettings.WithAppendOverlayMode(
                        x => x.WithKeyField(identityKeyField)
                            .WithMultiFieldOverlayBehaviour(overlayBehavior.ToMultiFieldOverlayBehaviour()));
                case ImportOverwriteMode.OverlayOnly:
                    return overlayModeSettings.WithOverlayMode(
                        x => x.WithKeyField(identityKeyField)
                            .WithMultiFieldOverlayBehaviour(overlayBehavior.ToMultiFieldOverlayBehaviour()));
                default:
                    throw new NotSupportedException($"ImportOverwriteMode {overwriteMode} is not supported.");
            }
        }

        private IWithFieldsMapping ConfigureFileImportSettings(
            IWithNatives nativesSettings,
            AdvancedImportSettings advancedSettings,
            bool imageImport,
            ImportNativeFileCopyMode nativeFileCopyMode,
            IReadOnlyList<FieldInfoDto> mappedFields)
        {
            _logger.LogInformation(
                "Configuring FileImport - ImageImport: {imageImport}, NativeFileCopyMode: {nativeFileCopyMode}", imageImport, nativeFileCopyMode);

            if (imageImport)
            {
                throw new NotSupportedException("Images import is not yet supported in IAPI2 pipeline");
            }
            else
            {
                if (nativeFileCopyMode == ImportNativeFileCopyMode.DoNotImportNativeFiles)
                {
                    return nativesSettings
                        .WithoutNatives()
                        .WithoutImages();
                }
                else
                {
                    if (nativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles)
                    {
                        advancedSettings.Other.Billable = true;
                        advancedSettings.Native.ValidateFileLocation = false;
                    }

                    ConfigureFileTypeSettings(mappedFields, advancedSettings);

                    int nativeFilePathIndex = GetFieldIndex(mappedFields, SpecialFieldType.NativeFileLocation);
                    int nativeFileNameIndex = GetFieldIndex(mappedFields, SpecialFieldType.NativeFileFilename);

                    return nativesSettings
                        .WithNatives(x => x
                            .WithFilePathDefinedInColumn(nativeFilePathIndex)
                            .WithFileNameDefinedInColumn(nativeFileNameIndex))
                        .WithoutImages();
                }
            }
        }

        private void ConfigureFileTypeSettings(IEnumerable<FieldInfoDto> mappedFields, AdvancedImportSettings advancedSettings)
        {
            advancedSettings.Native.FileSizeColumnIndex = GetFieldIndex(mappedFields, SpecialFieldType.NativeFileSize);
            advancedSettings.Native.FileType.SupportedByViewerColumnIndex = GetFieldIndex(mappedFields, SpecialFieldType.SupportedByViewer);
            advancedSettings.Native.FileType.RelativityNativeTypeColumnIndex = GetFieldIndex(mappedFields, SpecialFieldType.RelativityNativeType);
        }

        private IWithFolders ConfigureFieldsMappingSettings(IWithFieldsMapping fieldsMappingSettings, IEnumerable<FieldInfoDto> mappedFields)
        {
            _logger.LogInformation("Configuring FieldsMapping...");
            return fieldsMappingSettings.WithFieldsMapped(x =>
            {
                foreach (FieldInfoDto pair in mappedFields)
                {
                    if (pair.SpecialFieldType != SpecialFieldType.None)
                    {
                        _logger.LogInformation("Skip SpecialField - Name: {fieldName}", pair.SourceFieldName);
                        continue;
                    }

                    switch (pair.RelativityDataType)
                    {
                        case RelativityDataType.LongText:
                        case RelativityDataType.MultipleChoice:
                        case RelativityDataType.MultipleObject:
                            throw new NotSupportedException($"Mapping type {nameof(pair.RelativityDataType)} is not supported in IAPI 2.0");
                        default:
                            _logger.LogInformation(
                                "Configure Field - Index: {fieldIndex}, SourceName: {fieldName}, DestinationName",
                                pair.DocumentFieldIndex,
                                pair.SourceFieldName,
                                pair.DestinationFieldName);
                            x = x.WithField(pair.DocumentFieldIndex, pair.DestinationFieldName);
                            break;
                    }
                }
            });
        }

        private ImportDocumentSettings ConfigureDestinationFolderStructure(
            IWithFolders input,
            IEnumerable<FieldInfoDto> mappedFields,
            DestinationFolderStructureBehavior folderStructureBehavior,
            int destinationFolderArtifactId,
            string folderPathField)
        {
            switch (folderStructureBehavior)
            {
                case DestinationFolderStructureBehavior.None:
                    return input.WithFolders(f =>
                        f.WithRootFolderID(destinationFolderArtifactId, r =>
                            r.WithAllDocumentsInRootFolder()));

                case DestinationFolderStructureBehavior.ReadFromField:
                    return input.WithFolders(f =>
                        f.WithRootFolderID(destinationFolderArtifactId, r =>
                            r.WithFolderPathDefinedInColumn(
                                GetFieldIndex(mappedFields, folderPathField))));

                case DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure:
                    return input.WithFolders(f =>
                        f.WithRootFolderID(destinationFolderArtifactId, r =>
                            r.WithFolderPathDefinedInColumn(
                                GetFieldIndex(mappedFields, SpecialFieldType.FolderPath))));
                default:
                    throw new NotSupportedException($"Unknown {nameof(DestinationFolderStructureBehavior)} enum value: {folderStructureBehavior}");
            }
        }

        private int GetFieldIndex(IEnumerable<FieldInfoDto> mappedFields, SpecialFieldType specialFieldType)
            => mappedFields.Single(x => x.SpecialFieldType == specialFieldType).DocumentFieldIndex;

        private int GetFieldIndex(IEnumerable<FieldInfoDto> fieldMappings, string filedName)
        {
            FieldInfoDto field = fieldMappings.FirstOrDefault(x => x.SourceFieldName == filedName);
            return field?.DocumentFieldIndex ?? -1;
        }

        private Task<IReadOnlyList<FieldInfoDto>> RetrieveMappedFieldsAsync(bool isImageImport, CancellationToken token)
             => isImageImport
                ? _fieldManager.GetImageAllFieldsAsync(token)
                : _fieldManager.GetNativeAllFieldsAsync(token);
    }
}
