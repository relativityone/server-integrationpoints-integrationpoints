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
                IWithOverlayMode result = ImportDocumentSettingsBuilder.Create();
                IWithNatives withNatives = ConfigureOverwriteMode(result, configuration.ImportOverwriteMode,  MapOverlayBehavior(configuration.FieldOverlayBehavior));

                List<FieldInfoDto> allMappings = (await _fieldManager.GetNativeAllFieldsAsync(token.AnyReasonCancellationToken)).ToList();
                IWithFieldsMapping withFieldsMapping = ConfigureFileImport(
                    withNatives,
                    configuration.ImageImport,
                    GetFieldIndex(allMappings, GetSpecialFieldName(allMappings, SpecialFieldType.NativeFileLocation)),
                    GetFieldIndex(allMappings, GetSpecialFieldName(allMappings, SpecialFieldType.NativeFileFilename)));

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
                    Response importJobResponse = await importJobController.CreateAsync(
                            workspaceID: configuration.DestinationWorkspaceArtifactId,
                            importJobID: Guid.Parse(_parameters.WorkflowId),
                            applicationName: _parameters.SyncApplicationName,
                            correlationID: _parameters.WorkflowId)
                        .ConfigureAwait(false);

                    if (importJobResponse.IsSuccess == false)
                    {
                        _logger.LogError("Cannot create import job of id {importJobId}: {messageCode} {message}", importJobResponse.ImportJobID, importJobResponse.ErrorCode, importJobResponse.ErrorMessage);
                        return ExecutionResult.Failure(importJobResponse.ErrorMessage, null);
                    }

                    Response documentConfigurationResponse = await documentConfigurationController.CreateAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        Guid.Parse(_parameters.WorkflowId),
                        importSettings).ConfigureAwait(false);

                    if (documentConfigurationResponse.IsSuccess == false)
                    {
                        _logger.LogError("Cannot create document configuration: {messageCode} {message}", documentConfigurationResponse.ErrorCode, documentConfigurationResponse.ErrorMessage);
                        return ExecutionResult.Failure(documentConfigurationResponse.ErrorMessage, null);
                    }

                    Response jobBeginResponse = await importJobController
                        .BeginAsync(configuration.DestinationWorkspaceArtifactId, Guid.Parse(_parameters.WorkflowId))
                        .ConfigureAwait(false);

                    if (jobBeginResponse.IsSuccess == false)
                    {
                        _logger.LogError("Cannot begin import job: {messageCode} {message}", jobBeginResponse.ErrorCode, jobBeginResponse.ErrorMessage);
                        return ExecutionResult.Failure(jobBeginResponse.ErrorMessage, null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured when configuring IAPI2.0 document synchronization");
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
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

        private string GetSpecialFieldName(IList<FieldInfoDto> specialFields, SpecialFieldType specialFieldType)
        {
            FieldInfoDto specialField = specialFields.FirstOrDefault(x => x.SpecialFieldType == specialFieldType);

            if (specialField == null)
            {
                string message = $"Cannot find special field name: {specialFieldType}";
                _logger.LogError(message);
                throw new SyncException(message);
            }

            return specialField.DestinationFieldName;
        }

        private IWithNatives ConfigureOverwriteMode(IWithOverlayMode input, ImportOverwriteMode overwriteMode, MultiFieldOverlayBehaviour overlayBehavior)
        {
            switch (overwriteMode)
            {
                case ImportOverwriteMode.AppendOnly:
                    return input.WithAppendMode();

                case ImportOverwriteMode.AppendOverlay:
                    return input.WithAppendOverlayMode(x => x.WithKeyField(GetDestinationIdentityFieldName()).WithMultiFieldOverlayBehaviour(overlayBehavior));

                case ImportOverwriteMode.OverlayOnly:
                    return input.WithOverlayMode(x => x.WithKeyField(GetDestinationIdentityFieldName()).WithMultiFieldOverlayBehaviour(overlayBehavior));

                default:
                    throw new NotSupportedException();
            }
        }

        private IWithFieldsMapping ConfigureFileImport(IWithNatives input, bool imageImport, int nativeFilePathIndex, int nativeFileNameIndex)
        {
            if (imageImport)
            {
                throw new NotSupportedException("Images import is not supported in IAPI2 pipeline");
            }
            else
            {
                return input.WithNatives(x => x
                        .WithFilePathDefinedInColumn(nativeFilePathIndex)
                        .WithFileNameDefinedInColumn(nativeFileNameIndex))
                    .WithoutImages();
            }
        }

        private IWithFolders ConfigureFieldMappings(IWithFieldsMapping input, IList<FieldInfoDto> fieldMappings)
        {
            return input.WithFieldsMapped(x =>
            {
                foreach (FieldInfoDto mapping in fieldMappings)
                {
                    switch (mapping.RelativityDataType)
                    {
                        case RelativityDataType.SingleObject:
                            x = x.WithObjectFieldContainingArtifactID(mapping.DocumentFieldIndex, mapping.DestinationFieldName);
                            break;

                        case RelativityDataType.MultipleChoice:
                        case RelativityDataType.MultipleObject:
                            throw new NotSupportedException($"Mapping type {nameof(mapping.RelativityDataType)} is not supported in IAPI 2.0");

                        default:
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

        private static int GetFieldIndex(IList<FieldInfoDto> fieldMappings, string fieldName)
        {
            FieldInfoDto field = fieldMappings.FirstOrDefault(x => string.Equals(x.SourceFieldName, fieldName, StringComparison.InvariantCultureIgnoreCase));
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
