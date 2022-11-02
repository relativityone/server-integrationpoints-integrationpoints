using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Builders.Documents;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ConfigureDocumentSynchronizationExecutor : IExecutor<IConfigureDocumentSynchronizationConfiguration>
    {
        private readonly SyncJobParameters _parameters;
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IFieldMappings _fieldMappings;
        private readonly IFieldManager _fieldManager;
        private readonly IAPILog _logger;

        public ConfigureDocumentSynchronizationExecutor(
            SyncJobParameters parameters,
            IDestinationServiceFactoryForUser serviceFactory,
            IFieldMappings fieldMappings,
            IFieldManager fieldManager,
            IAPILog logger)
        {
            _parameters = parameters;
            _serviceFactory = serviceFactory;
            _fieldMappings = fieldMappings;
            _fieldManager = fieldManager;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                _logger.LogInformation("Creating ImportDocumentSettingsBuilder...");
                IWithOverlayMode result = ImportDocumentSettingsBuilder.Create();
                IWithNatives withNatives = ConfigureOverwriteMode(result, configuration.ImportOverwriteMode, MapOverlayBehavior(configuration.FieldOverlayBehavior));

                List<FieldInfoDto> allMappings = (await _fieldManager.GetNativeAllFieldsAsync(token.AnyReasonCancellationToken)).ToList();
                _logger.LogInformation("FieldsMapping retrieved - Fields Count: {mappingsCount}", allMappings.Count);

                IWithFieldsMapping withFieldsMapping = ConfigureFileImport(
                    withNatives,
                    configuration.ImageImport,
                    configuration.ImportNativeFileCopyMode,
                    allMappings);

                IWithFolders withFolders = ConfigureFieldMappings(withFieldsMapping, allMappings);

                ImportDocumentSettings importSettings = ConfigureDestinationFolderStructure(
                    withFolders,
                    configuration.DestinationFolderStructureBehavior,
                    configuration.DataDestinationArtifactId,
                    GetFieldIndex(allMappings, configuration.FolderPathField));

                using (IImportJobController importJobController = await _serviceFactory
                           .CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                using (IDocumentConfigurationController documentConfigurationController = await _serviceFactory
                           .CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
                {
                    Guid importJobId = configuration.ExportRunId;

                    _logger.LogInformation("Creating Import Job with {importJobId} ID...", importJobId);

                    Response importJobResponse = await importJobController.CreateAsync(
                            workspaceID: configuration.DestinationWorkspaceArtifactId,
                            importJobID: importJobId,
                            applicationName: _parameters.SyncApplicationName,
                            correlationID: _parameters.WorkflowId)
                        .ConfigureAwait(false);

                    ValidateResponse(importJobResponse);

                    _logger.LogInformation("Load ImportDocumentsSettings to Job {jobId}", importJobId);

                    Response documentConfigurationResponse = await documentConfigurationController.CreateAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        importJobId,
                        importSettings).ConfigureAwait(false);

                    ValidateResponse(documentConfigurationResponse);

                    _logger.LogInformation("Start ImportJob {jobId} and wait for DataSources...", importJobId);

                    Response jobBeginResponse = await importJobController
                        .BeginAsync(configuration.DestinationWorkspaceArtifactId, importJobId)
                        .ConfigureAwait(false);

                    ValidateResponse(jobBeginResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured when configuring IAPI2.0 document synchronization");
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
        }

        private void ValidateResponse(Response response)
        {
            if (response.IsSuccess == false)
            {
                string message = $"ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}";
                throw new SyncException(message);
            }
        }

        private string GetDestinationIdentityFieldName()
        {
            FieldMap destinationIdentityField = _fieldMappings.GetFieldMappings().FirstOrDefault(x => x.DestinationField.IsIdentifier);
            if (destinationIdentityField == null)
            {
                const string message = "Cannot find destination identifier field in field mappings.";
                _logger.LogError(message);
                throw new SyncException(message);
            }

            return destinationIdentityField.DestinationField.DisplayName;
        }

        private IWithNatives ConfigureOverwriteMode(IWithOverlayMode input, ImportOverwriteMode overwriteMode, MultiFieldOverlayBehaviour overlayBehavior)
        {
            _logger.LogInformation("Configuring OverwriteMode - OverwriteMode: {overwriteMode}, OverlayBehavior: {overlayBehavior}", overwriteMode, overlayBehavior);
            switch (overwriteMode)
            {
                case ImportOverwriteMode.AppendOnly:
                    return input.WithAppendMode();

                case ImportOverwriteMode.AppendOverlay:
                    return input.WithAppendOverlayMode(x => x.WithKeyField(GetDestinationIdentityFieldName()).WithMultiFieldOverlayBehaviour(overlayBehavior));

                case ImportOverwriteMode.OverlayOnly:
                    return input.WithOverlayMode(x => x.WithKeyField(GetDestinationIdentityFieldName()).WithMultiFieldOverlayBehaviour(overlayBehavior));

                default:
                    throw new NotSupportedException($"ImportOverwriteMode {overwriteMode} is not supported.");
            }
        }

        private IWithFieldsMapping ConfigureFileImport(
            IWithNatives input,
            bool imageImport,
            ImportNativeFileCopyMode nativeFileCopyMode,
            List<FieldInfoDto> fieldsMapping)
        {
            _logger.LogInformation(
                "Configuring FileImport - " +
                "ImageImport: {imageImport}, " +
                "NativeFileCopyMode: {nativeFileCopyMode}",
                imageImport,
                nativeFileCopyMode);

            if (imageImport)
            {
                throw new NotSupportedException("Images import is not supported in IAPI2 pipeline");
            }
            else
            {
                if (nativeFileCopyMode == ImportNativeFileCopyMode.DoNotImportNativeFiles)
                {
                    return input
                        .WithoutNatives()
                        .WithoutImages();
                }
                else
                {
                    int nativeFilePathIndex = GetFieldIndex(fieldsMapping, SpecialFieldType.NativeFileLocation);
                    int nativeFileNameIndex = GetFieldIndex(fieldsMapping, SpecialFieldType.NativeFileFilename);

                    return input.WithNatives(x => x
                            .WithFilePathDefinedInColumn(nativeFilePathIndex)
                            .WithFileNameDefinedInColumn(nativeFileNameIndex))
                        .WithoutImages();
                }
            }
        }

        private IWithFolders ConfigureFieldMappings(IWithFieldsMapping input, IList<FieldInfoDto> fieldMappings)
        {
            _logger.LogInformation("Configuring FieldsMapping...");
            return input.WithFieldsMapped(x =>
            {
                foreach (FieldInfoDto mapping in fieldMappings)
                {
                    if(mapping.SpecialFieldType != SpecialFieldType.None)
                    {
                        _logger.LogInformation("Skip SpecialField - Name: {fieldName}", mapping.SourceFieldName);
                        continue;
                    }

                    switch (mapping.RelativityDataType)
                    {
                        case RelativityDataType.SingleObject:
                            x = x.WithObjectFieldContainingArtifactID(mapping.DocumentFieldIndex, mapping.DestinationFieldName);
                            break;

                        case RelativityDataType.MultipleChoice:
                        case RelativityDataType.MultipleObject:
                            throw new NotSupportedException($"Mapping type {nameof(mapping.RelativityDataType)} is not supported in IAPI 2.0");

                        default:
                            _logger.LogInformation("Configure Field - Index: {fieldIndex}, SourceName: {fieldName}, DestinationName",
                                mapping.DocumentFieldIndex, mapping.SourceFieldName, mapping.DestinationFieldName);
                            x = x.WithField(mapping.DocumentFieldIndex, mapping.DestinationFieldName);
                            break;
                    }
                }
            });
        }

        private ImportDocumentSettings ConfigureDestinationFolderStructure(IWithFolders input, DestinationFolderStructureBehavior folderStructureBehavior, int destinationFolderArtifactId, int folderPathIndex)
        {
            switch (folderStructureBehavior)
            {
                case DestinationFolderStructureBehavior.None:
                    return input.WithoutFolders();

                case DestinationFolderStructureBehavior.ReadFromField:
                    return input.WithFolders(f => f
                        .WithRootFolderID(destinationFolderArtifactId, r => r
                            .WithFolderPathDefinedInColumn(folderPathIndex)));

                default:
                    throw new NotSupportedException($"Unknown {nameof(DestinationFolderStructureBehavior)} enum value: {folderStructureBehavior}");
            }
        }

        private static int GetFieldIndex(IList<FieldInfoDto> fieldMappings, SpecialFieldType specialFieldType)
        {
            FieldInfoDto field = fieldMappings.FirstOrDefault(x => x.SpecialFieldType == specialFieldType);
            return field?.DocumentFieldIndex ?? -1;
        }

        private static int GetFieldIndex(IList<FieldInfoDto> fieldMappings, string filedName)
        {
            FieldInfoDto field = fieldMappings.FirstOrDefault(x => x.SourceFieldName == filedName);
            return field?.DocumentFieldIndex ?? -1;
        }

        private static MultiFieldOverlayBehaviour MapOverlayBehavior(FieldOverlayBehavior behavior)
        {
            switch (behavior)
            {
                case FieldOverlayBehavior.MergeValues: return MultiFieldOverlayBehaviour.MergeAll;
                case FieldOverlayBehavior.ReplaceValues: return MultiFieldOverlayBehaviour.ReplaceAll;
                case FieldOverlayBehavior.UseFieldSettings: return MultiFieldOverlayBehaviour.UseRelativityDefaults;

                default: throw new NotSupportedException($"Unknown {nameof(FieldOverlayBehavior)} enum value: {behavior}");
            }
        }
    }
}
