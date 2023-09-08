using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.AdlsHelpers;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.WinEDDS.Api;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.AutomatedWorkflows.SDK;
using Relativity.AutomatedWorkflows.SDK.V2.Models.Triggers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class ImportServiceManager : ServiceManagerBase
    {
        private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000014;
        private const string _AUTOMATED_WORKFLOWS_APPLICATION_NAME = "Automated Workflows";
        private readonly IHelper _helper;
        private readonly IRetryHandler _automatedWorkflowsRetryHandler;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IImportFileLocationService _importFileLocationService;
        private readonly IJobStatusUpdater _jobStatusUpdater;
        private readonly IAutomatedWorkflowsManager _automatedWorkflowsManager;
        private readonly IJobTracker _jobTracker;
        private readonly IRipToggleProvider _toggleProvider;
        private readonly IFileIdentificationService _fileIdentificationService;
        private readonly IDataTransferLocationService _dataTransferLocationService;
        private readonly IAdlsHelper _adlsHelper;
        private readonly ILogger<ImportServiceManager> _logger;
        private readonly object _syncRoot = new object();
        public const string RAW_STATE_COMPLETE_WITH_ERRORS = "complete-with-errors";
        public const string RAW_STATE_COMPLETE = "complete";
        public const string RAW_RIP_TRIGGER_NAME = "relativity@on-new-documents-added";
        public const string RAW_TRIGGER_INPUT_ID = "type";
        public const string RAW_TRIGGER_INPUT_VALUE = Constants.IntegrationPoints.APPLICATION_NAME;

        public ImportServiceManager(
            IHelper helper,
            IRetryHandler retryHandler,
            ICaseServiceContext caseServiceContext,
            ISynchronizerFactory synchronizerFactory,
            IManagerFactory managerFactory,
            IEnumerable<IBatchStatus> statuses,
            ISerializer serializer,
            IJobService jobService,
            IScheduleRuleFactory scheduleRuleFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobStatisticsService statisticsService,
            IDataReaderFactory dataReaderFactory,
            IImportFileLocationService importFileLocationService,
            IAgentValidator agentValidator,
            IIntegrationPointService integrationPointService,
            IJobStatusUpdater jobStatusUpdater,
            IAutomatedWorkflowsManager automatedWorkflowsManager,
            IJobTracker jobTracker,
            IRipToggleProvider toggleProvider,
            IFileIdentificationService fileIdentificationService,
            IDataTransferLocationService dataTransferLocationService,
            IAdlsHelper adlsHelper,
            ILogger<ImportServiceManager> logger,
            IDiagnosticLog diagnosticLog)
            : base(
                helper,
                jobService,
                serializer,
                jobHistoryService,
                jobHistoryErrorService,
                scheduleRuleFactory,
                managerFactory,
                statuses,
                caseServiceContext,
                statisticsService,
                synchronizerFactory,
                agentValidator,
                integrationPointService,
                logger.ForContext<ServiceManagerBase>(),
                diagnosticLog)
        {
            _helper = helper;
            _automatedWorkflowsRetryHandler = retryHandler;
            _dataReaderFactory = dataReaderFactory;
            _importFileLocationService = importFileLocationService;
            _jobStatusUpdater = jobStatusUpdater;
            _automatedWorkflowsManager = automatedWorkflowsManager;
            _jobTracker = jobTracker;
            _toggleProvider = toggleProvider;
            _fileIdentificationService = fileIdentificationService;
            _dataTransferLocationService = dataTransferLocationService;
            _adlsHelper = adlsHelper;
            _logger = logger;
        }

        public override void Execute(Job job)
        {
            try
            {
                LogExecuteStart(job);

                InitializeService(job, supportsDrainStop: true);

                JobStopManager.ThrowIfStopRequested();

                IDataSynchronizer synchronizer = CreateDestinationProvider(IntegrationPointDto.DestinationConfiguration);

                JobStopManager.ThrowIfStopRequested();

                SetupSubscriptions(synchronizer, job);

                JobStopManager.ThrowIfStopRequested();

                ImportSettings settings = GetImportApiSettingsObjectForUser(job);

                DiagnosticLog.LogDiagnostic("ImportSettings: {@importSettings}", settings);

                string providerSettings = UpdatedProviderSettingsLoadFile();

                DiagnosticLog.LogDiagnostic("ProviderSettings: {settings}", providerSettings);

                LogWorkspaceFileShareTypeAsync(job).GetAwaiter().GetResult();

                int sourceRecordCount = UpdateSourceRecordCount(settings);
                if (sourceRecordCount > 0)
                {
                    bool processExtraFileMetadata = ShouldProcessExtraFileMetadata();
                    if (processExtraFileMetadata)
                    {
                        GatherFileMetadataAsync().GetAwaiter().GetResult();
                    }

                    using (var context = new ImportTransferDataContext(
                               _dataReaderFactory,
                               providerSettings,
                               IntegrationPointDto.FieldMappings,
                               JobStopManager,
                               processExtraFileMetadata))
                    {
                        context.TransferredItemsCount = JobHistory.ItemsTransferred ?? 0;
                        context.FailedItemsCount = JobHistory.ItemsWithErrors ?? 0;

                        DiagnosticLog.LogDiagnostic("Context: {@context}", context);

                        DiagnosticLog.LogDiagnostic("Synchronizing...");
                        synchronizer.SyncData(context, IntegrationPointDto.FieldMappings, settings, JobStopManager, DiagnosticLog);
                        DiagnosticLog.LogDiagnostic("Finished synchronizing.");
                    }
                }

                MarkJobAsDrainStoppedIfNeeded(job);

                LogExecuteSuccesfulEnd(job);
            }
            catch (OperationCanceledException e)
            {
                LogJobStoppedException(job, e);
                // ignore error.
            }
            catch (Exception ex)
            {
                HandleGenericException(ex, job);
                if (ex is PermissionException || ex is IntegrationPointValidationException || ex is IntegrationPointsException)
                {
                    throw;
                }
            }
            finally
            {
                SetJobStateAsUnstoppableIfNeeded(job);
                JobHistoryErrorService.CommitErrors();
                FinalizeService(job);
                LogExecuteFinalize(job);
                SendAutomatedWorkflowsTriggerAsync(job).GetAwaiter().GetResult();
            }
        }

        private async Task LogWorkspaceFileShareTypeAsync(Job job)
        {
            if (IntegrationPointDto.DestinationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles)
            {
                bool? isWorkspaceOnAdls = await _adlsHelper.IsWorkspaceMigratedToAdlsAsync(job.WorkspaceID).ConfigureAwait(false);
                _logger.LogInformation("Workspace ID: {workspaceId} is migrated to ADLS: {isAdls}", job.WorkspaceID, isWorkspaceOnAdls);
            }
        }

        private bool ShouldProcessExtraFileMetadata()
        {
            bool useCalToggleValue = _toggleProvider.IsEnabled<UseCalInLegacyTapiToggle>();
            var nativeCopyMode = IntegrationPointDto.DestinationConfiguration.ImportNativeFileCopyMode;
            return useCalToggleValue && nativeCopyMode == ImportNativeFileCopyModeEnum.CopyFiles;
        }

        private async Task GatherFileMetadataAsync()
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();

                var importProviderSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
                string workspaceFileShareDirectory = _dataTransferLocationService.GetWorkspaceFileLocationRootPath(importProviderSettings.WorkspaceId);

                using (INativeFilePathReader nativeFilePathReader = _dataReaderFactory.GetNativeFilePathReader(
                           IntegrationPointDto.FieldMappings.ToArray(),
                           IntegrationPointDto.SourceConfiguration,
                           JobStopManager))
                {
                    var nativeFiles = new BlockingCollection<string>();
                    Task identificationTask = _fileIdentificationService.IdentifyFilesAsync(nativeFiles);

                    // the first row contains header with column names, let's skip it
                    nativeFilePathReader.Read();

                    while (nativeFilePathReader.Read())
                    {
                        string fullPath = Path.Combine(workspaceFileShareDirectory, nativeFilePathReader.GetCurrentNativeFilePath());
                        DiagnosticLog.LogDiagnostic("Reading Native File - {nativeFile}", fullPath);
                        nativeFiles.Add(fullPath);
                    }

                    nativeFiles.CompleteAdding();

                    await identificationTask.ConfigureAwait(false);
                }

                watch.Stop();
                _logger.LogInformation("Files metadata gathered within {seconds} seconds", watch.Elapsed.Seconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to gather file metadata", ex);
            }
        }

        protected override void FinalizeService(Job job)
        {
            base.FinalizeService(job);
            RemoveTrackingEntry(job, Identifier, !IsDrainStopped());
        }

        private void RemoveTrackingEntry(Job job, Guid batchId, bool isBatchFinished)
        {
            _logger.LogInformation("Removing tracking entry for job {jobId} BatchID: {batchId} and isBatchFinished: {isBatchFinished}", job.JobId, batchId, isBatchFinished);
            _jobTracker.CheckEntries(job, batchId.ToString(), isBatchFinished);
        }

        protected override void RunValidation(Job job)
        {
            base.RunValidation(job);

            ValidateLoadFile(job);
        }

        private void MarkJobAsDrainStoppedIfNeeded(Job job)
        {
            Guid batchInstance = Guid.Parse(JobHistory.BatchInstance);
            JobHistory = JobHistoryService.GetRdoWithoutDocuments(batchInstance);
            int processedItemsCount = GetProcessedItemsCount(JobHistory);

            DiagnosticLog.LogDiagnostic("Processed ItemsCount: {itemsCount}", processedItemsCount);

            if (IsDrainStopped())
            {
                _logger.LogInformation("ImportServiceManager job {jobId} was DrainStopped.", job.JobId);
                if (AnyItemsLeftToBeProcessed(processedItemsCount, JobHistory))
                {
                    UpdateJobWithProcessedItemsCount(job, processedItemsCount);
                    return;
                }

                JobService.UpdateStopState(new List<long> { job.JobId }, StopState.None);
            }
        }

        private void UpdateJobWithProcessedItemsCount(Job job, int processedItemsCount)
        {
            _logger.LogInformation("Update Job Details {jobId} with processed items {processedItemsCount}", job.JobId, processedItemsCount);
            TaskParameters updatedTaskParameters = UpdateJobDetails(job, processedItemsCount);
            job.JobDetails = Serializer.Serialize(updatedTaskParameters);
            JobService.UpdateJobDetails(job);
        }

        private TaskParameters UpdateJobDetails(Job job, int processedItemCount)
        {
            TaskParameters taskParameters = GetTaskParameters(job);

            LoadFileTaskParameters loadFileTaskParameters = GetLoadFileTaskParameters(taskParameters);
            loadFileTaskParameters.ProcessedItemsCount = processedItemCount;

            taskParameters.BatchParameters = loadFileTaskParameters;

            return taskParameters;
        }

        private bool IsDrainStopped()
        {
            return JobStopManager?.ShouldDrainStop == true;
        }

        private static int GetProcessedItemsCount(JobHistory jobHistory)
        {
            return (jobHistory.ItemsTransferred ?? 0) + (jobHistory.ItemsWithErrors ?? 0);
        }

        private bool AnyItemsLeftToBeProcessed(int processedItemCount, JobHistory jobHistory)
        {
            long totalItems = (jobHistory.TotalItems ?? int.MaxValue);

            _logger.LogInformation("Checking if some documents left to process {processedItemsCount}/{totalItemsCount}", processedItemCount, totalItems);

            return processedItemCount < totalItems;
        }

        private async Task SendAutomatedWorkflowsTriggerAsync(Job job)
        {
            JobHistory jobHistory = JobHistoryService.GetRdoWithoutDocuments(new Guid(job.CorrelationID));
            ChoiceRef status = _jobStatusUpdater.GenerateStatus(jobHistory);

            if (status.EqualsToChoice(JobStatusChoices.JobHistoryCompleted))
            {
                await SendAutomatedWorkflowsTriggerAsync(job.WorkspaceID, RAW_RIP_TRIGGER_NAME, RAW_STATE_COMPLETE).ConfigureAwait(false);
            }
            else if (status.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors))
            {
                await SendAutomatedWorkflowsTriggerAsync(job.WorkspaceID, RAW_RIP_TRIGGER_NAME, RAW_STATE_COMPLETE_WITH_ERRORS).ConfigureAwait(false);
            }
        }

        private async Task SendAutomatedWorkflowsTriggerAsync(int workspaceId, string triggerName, string state)
        {
            try
            {
                _logger.LogInformation("For workspace artifact ID : {AutomatedWorkflow.DestinationWorkspaceArtifactId} {AutomatedWorkflow.TriggerName} trigger {AutomatedWorkflow.TriggerValue} called with status {AutomatedWorkflow.Status}.", workspaceId, triggerName, RAW_TRIGGER_INPUT_VALUE, state);

                if (!await IsAutomatedWorkflowsInstalledAsync(workspaceId).ConfigureAwait(false))
                {
                    _logger.LogInformation(_AUTOMATED_WORKFLOWS_APPLICATION_NAME + " isn't installed in workspace {workspaceArtifactId}.", workspaceId);

                    return;
                }

                SendTriggerBody body = new SendTriggerBody
                {
                    Inputs = new List<TriggerInput>
                    {
                        new TriggerInput
                        {
                            ID = RAW_TRIGGER_INPUT_ID,
                            Value = RAW_TRIGGER_INPUT_VALUE,
                        },
                    },
                    State = state,
                };

                await _automatedWorkflowsRetryHandler.ExecuteWithRetriesAsync(async () =>
                {
                    await _automatedWorkflowsManager.SendTriggerAsync(workspaceId, triggerName, body).ConfigureAwait(false);
                }).ConfigureAwait(false);

                _logger.LogInformation("For workspace : {0} trigger {1} finished sending.", workspaceId, triggerName);
            }
            catch (Exception ex)
            {
                string message = "Error occured while executing trigger : {0} for workspace artifact ID : {1}";
                _logger.LogError(ex, message, triggerName, workspaceId);
            }
        }

        private async Task<bool> IsAutomatedWorkflowsInstalledAsync(int workspaceId)
        {
            using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                QueryRequest automatedWorkflowsInstalledRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID },
                    Condition = $"'Name' == '{_AUTOMATED_WORKFLOWS_APPLICATION_NAME}'",
                };
                QueryResultSlim automatedWorkflowsInstalledResult = await objectManager.QuerySlimAsync(workspaceId, automatedWorkflowsInstalledRequest, 0, 0).ConfigureAwait(false);

                return automatedWorkflowsInstalledResult.TotalCount > 0;
            }
        }

        private ImportSettings GetImportApiSettingsObjectForUser(Job job)
        {
            LogGetImportApiSettingsObjectForUserStart(job);
            ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
            ImportSettings importSettings = new ImportSettings(IntegrationPointDto.DestinationConfiguration);
            importSettings.CorrelationId = ImportSettings.CorrelationId;
            importSettings.JobID = ImportSettings.JobID;
            importSettings.DestinationConfiguration.Provider = nameof(ProviderType.ImportLoadFile);
            importSettings.ErrorFilePath = _importFileLocationService.ErrorFilePath(
                IntegrationPointDto.ArtifactId,
                IntegrationPointDto.Name,
                IntegrationPointDto.SourceConfiguration,
                IntegrationPointDto.DestinationConfiguration);

            // For LoadFile imports, correct an off-by-one error introduced by WinEDDS.LoadFileReader interacting with
            // ImportAPI process. This is introduced by the fact that the first record is the column header row.
            // Opticon files have no column header row
            if (importSettings.DestinationConfiguration.ImageImport)
            {
                importSettings.StartRecordNumber = int.Parse(providerSettings.LineNumber);
            }
            else
            {
                importSettings.StartRecordNumber = int.Parse(providerSettings.LineNumber) + 1;
            }

            int processedItemsCount = GetProcessedItemsCountFromJobDetails(job);
            importSettings.StartRecordNumber += processedItemsCount;

            importSettings.DestinationConfiguration.DestinationFolderArtifactId = providerSettings.DestinationFolderArtifactId;

            // Copy multi-value and nested delimiter settings chosen on configuration page to importAPI settings
            importSettings.MultiValueDelimiter = (char)providerSettings.AsciiMultiLine;
            importSettings.NestedValueDelimiter = (char)providerSettings.AsciiNestedValue;
            LogGetImportApiSettingsObjectForUserSuccesfulEnd(job);
            return importSettings;
        }

        private int UpdateSourceRecordCount(ImportSettings settings)
        {
            LogUpdateSourceRecordCountStart();
            // Cannot re-use the LoadFileDataReader once record count has been obtained (error file is not created properly due to an off-by-one error)
            using (IDataReader sourceReader = _dataReaderFactory.GetDataReader(
                       IntegrationPointDto.FieldMappings.ToArray(),
                       IntegrationPointDto.SourceConfiguration,
                       JobStopManager,
                       addExtraNativeColumns: false))
            {
                int recordCount = settings.DestinationConfiguration.ImageImport ?
                    (int)((IOpticonDataReader)sourceReader).CountRecords() :
                    (int)((IArtifactReader)sourceReader).CountRecords();

                lock (_syncRoot)
                {
                    JobHistory = JobHistoryService.GetRdoWithoutDocuments(Identifier);
                    JobHistory.TotalItems = recordCount;
                    UpdateJobStatus(JobHistory);

                    DiagnosticLog.LogDiagnostic("Update JobHistory with TotalItems {recordsCount}", recordCount);
                }

                LogUpdateSourceRecordSuccesfulEnd();
                return recordCount;
            }
        }

        private string UpdatedProviderSettingsLoadFile()
        {
            ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
            providerSettings.LoadFile = _importFileLocationService.LoadFileInfo(IntegrationPointDto.SourceConfiguration, IntegrationPointDto.DestinationConfiguration).FullPath;
            return Serializer.Serialize(providerSettings);
        }

        private void ValidateLoadFile(Job job)
        {
            LoadFileTaskParameters storedLoadFileParameters = GetLoadFileTaskParameters(GetTaskParameters(job));
            LoadFileInfo currentLoadFile = _importFileLocationService.LoadFileInfo(IntegrationPointDto.SourceConfiguration, IntegrationPointDto.DestinationConfiguration);

            _logger.LogInformation("Validating LoadFile {@loadFile}, based on TaskParameters {@taskParameters}",
                currentLoadFile, storedLoadFileParameters);

            if (storedLoadFileParameters == null)
            {
                _logger.LogWarning("TaskParameters doesn't contain LoadFile parameters, but should.");
                UpdateJobWithLoadFileDetails(job, currentLoadFile);
            }
            else
            {
                ValidateLoadFileHasNotChanged(storedLoadFileParameters, currentLoadFile);
            }
        }

        private TaskParameters GetTaskParameters(Job job)
        {
            TaskParameters parameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            return parameters;
        }

        private LoadFileTaskParameters GetLoadFileTaskParameters(TaskParameters parameters)
        {
            LoadFileTaskParameters loadFileParameters;
            if (parameters.BatchParameters is JObject)
            {
                loadFileParameters = ((JObject)parameters.BatchParameters).ToObject<LoadFileTaskParameters>();
            }
            else
            {
                loadFileParameters = (LoadFileTaskParameters)parameters.BatchParameters;
            }

            return loadFileParameters;
        }

        private void UpdateJobWithLoadFileDetails(Job job, LoadFileInfo loadFile)
        {
            _logger.LogInformation("Updating Job {jobId} details with LoadFileInfo {@loadFile}", job.JobId, loadFile);
            TaskParameters taskParameters = GetTaskParameters(job);
            taskParameters.BatchParameters = new LoadFileTaskParameters
            {
                LastModifiedDate = loadFile.LastModifiedDate,
                Size = loadFile.Size
            };

            job.JobDetails = Serializer.Serialize(taskParameters);

            JobService.UpdateJobDetails(job);
        }

        private void ValidateLoadFileHasNotChanged(LoadFileTaskParameters storedLoadFileParameters, LoadFileInfo currentLoadFile)
        {
            _logger.LogInformation("Validating if LoadFile has not changed since the job was scheduled...");
            if (currentLoadFile.Size != storedLoadFileParameters.Size || currentLoadFile.LastModifiedDate != storedLoadFileParameters.LastModifiedDate)
            {
                ValidationResult result = new ValidationResult(false, "Load File has been modified.");
                throw new IntegrationPointValidationException(result);
            }
        }

        private int GetProcessedItemsCountFromJobDetails(Job job)
        {
            LoadFileTaskParameters loadFileTaskParameters = GetLoadFileTaskParameters(GetTaskParameters(job));
            return loadFileTaskParameters.ProcessedItemsCount;
        }

        #region Logging
        private void LogExecuteFinalize(Job job)
        {
            _logger.LogInformation("Finalized execution of job in Import Service Manager. job: {JobId}.", job.JobId);
        }

        private void LogExecuteSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Succesfully finished execution of job in Import Service Manager. job: {JobId}.", job.JobId);
        }

        private void LogExecuteStart(Job job)
        {
            _logger.LogInformation("Starting execution of job in Import Service Manager. job: {JobId}.", job.JobId);
        }

        private void LogGetImportApiSettingsObjectForUserSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Succesfully finished getting Import API settings for user. job: {JobId}.", job.JobId);
        }

        private void LogGetImportApiSettingsObjectForUserStart(Job job)
        {
            _logger.LogInformation("Getting Import API settings for user. job: {JobId}.", job.JobId);
        }

        private void LogUpdateSourceRecordSuccesfulEnd()
        {
            _logger.LogInformation("Succesfully finished updating source record count.");
        }

        private void LogUpdateSourceRecordCountStart()
        {
            _logger.LogInformation("Started updating source record count.");
        }
        #endregion
    }
}
