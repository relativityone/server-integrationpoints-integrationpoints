using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Toggles;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class ExportServiceManager : ServiceManagerBase
    {
        private int _sourceSavedSearchArtifactID;
        private int? _itemLevelErrorSavedSearchArtifactID;
        private List<IBatchStatus> _exportServiceJobObservers;
        private readonly IExportServiceObserversFactory _exportServiceObserversFactory;
        private readonly IExporterFactory _exporterFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IToggleProvider _toggleProvider;
        private readonly IDocumentRepository _documentRepository;
        private readonly IExportDataSanitizer _exportDataSanitizer;
        private readonly object _syncRoot = new object();
        private IJobHistoryErrorManager JobHistoryErrorManager { get; set; }

        private JobHistoryErrorDTO.UpdateStatusType UpdateStatusType { get; set; }

        public ExportServiceManager(
            IHelper helper,
            ICaseServiceContext caseServiceContext,
            ISynchronizerFactory synchronizerFactory,
            IExporterFactory exporterFactory,
            IExportServiceObserversFactory exportServiceObserversFactory,
            IRepositoryFactory repositoryFactory,
            IManagerFactory managerFactory,
            IEnumerable<IBatchStatus> statuses,
            ISerializer serializer,
            IJobService jobService,
            IScheduleRuleFactory scheduleRuleFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobStatisticsService statisticsService,
            IToggleProvider toggleProvider,
            IAgentValidator agentValidator,
            IIntegrationPointService integrationPointService,
            IDocumentRepository documentRepository,
            IExportDataSanitizer exportDataSanitizer,
            IDiagnosticLog diagnosticLog)
            : base(helper,
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
                diagnosticLog)
        {
            _repositoryFactory = repositoryFactory;
            _toggleProvider = toggleProvider;
            _exportServiceObserversFactory = exportServiceObserversFactory;
            _exporterFactory = exporterFactory;
            _documentRepository = documentRepository;
            _exportDataSanitizer = exportDataSanitizer;
            Logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceManager>();
        }

        public override void Execute(Job job)
        {
            try
            {
                LogExecuteStart(job);
                InitializeService(job, supportsDrainStop: false);
                JobStopManager.ThrowIfStopRequested();

                string destinationConfig = IntegrationPointDto.DestinationConfiguration;
                string userImportApiSettings = GetImportApiSettingsForUser(job, destinationConfig);
                IDataSynchronizer synchronizer = CreateDestinationProvider(destinationConfig);

                try
                {
                    JobStopManager.ThrowIfStopRequested();

                    InitializeExportServiceObservers(job, userImportApiSettings);
                    SetupSubscriptions(synchronizer, job);

                    JobStopManager.ThrowIfStopRequested();

                    PushDocuments(job, userImportApiSettings, synchronizer);
                }
                finally
                {
                    FinalizeExportServiceObservers(job);
                }
            }
            catch (OperationCanceledException e)
            {
                LogJobStoppedException(job, e);
                // ignore error.
            }
            catch (Exception ex)
            {
                HandleGenericException(ex, job);

                IExtendedJob extendedJob =
                    new ExtendedJob(job, JobHistoryService, IntegrationPointService, Serializer, Logger);
                IJobHistoryRepository jobHistoryRepository =
                    _repositoryFactory.GetJobHistoryRepository(extendedJob.WorkspaceId);
                try
                {
                    if (ex is IntegrationPointValidationException)
                    {
                        jobHistoryRepository.MarkJobAsValidationFailed(extendedJob.JobHistoryId,
                            extendedJob.IntegrationPointId, DateTime.UtcNow);
                    }
                    else
                    {
                        jobHistoryRepository.MarkJobAsFailed(extendedJob.JobHistoryId,
                            extendedJob.IntegrationPointId, DateTime.UtcNow);
                    }
                }
                catch (Exception)
                {
                }

                if (ex is PermissionException || ex is IntegrationPointValidationException || ex is IntegrationPointsException)
                {
                    throw;
                }
            }
            finally
            {
                SetJobStateAsUnstoppableIfNeeded(job);
                JobHistoryErrorService.CommitErrors();
                FinalizeExportService(job);
                FinalizeService(job);
                LogExecuteEnd(job);
            }
        }

        private void PushDocuments(Job job, string userImportApiSettings, IDataSynchronizer synchronizer)
        {
            int savedSearchID = UpdateStatusType.IsItemLevelErrorRetry()
                ? _itemLevelErrorSavedSearchArtifactID.Value
                : _sourceSavedSearchArtifactID;

            using (IExporterService exporter = _exporterFactory.BuildExporter(
                JobStopManager,
                IntegrationPointDto.FieldMappings.ToArray(),
                IntegrationPointDto.SourceConfiguration,
                savedSearchID,
                userImportApiSettings,
                _documentRepository,
                _exportDataSanitizer))
            {
                LogPushingDocumentsStart(job);
                IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
                    .Select(observer => observer.ScratchTableRepository).ToArray();

                var exporterTransferConfiguration = new ExporterTransferConfiguration(scratchTables, JobHistoryService,
                    Identifier, Serializer.Deserialize<ImportSettings>(userImportApiSettings));

                IDataTransferContext dataTransferContext = exporter.GetDataTransferContext(exporterTransferConfiguration);

                lock (_syncRoot)
                {
                    JobHistory = JobHistoryService.GetRdoWithoutDocuments(Identifier);
                    dataTransferContext.UpdateTransferStatus();
                }

                int totalRecords = exporter.TotalRecordsFound;
                if (totalRecords > 0)
                {
                    Logger.LogInformation("Start pushing documents. Number of records found: {numberOfRecordsFound}", totalRecords);

                    synchronizer.SyncData(dataTransferContext, IntegrationPointDto.FieldMappings, userImportApiSettings, JobStopManager, DiagnosticLog);
                }
                LogPushingDocumentsSuccessfulEnd(job);
            }
        }

        protected override void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
        {
            base.SetupSubscriptions(synchronizer, job);
            IScratchTableRepository[] scratchTableToMonitorItemLevelError =
                _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
                    .Where(observer => observer.ScratchTableRepository.IgnoreErrorDocuments == false)
                    .Select(observer => observer.ScratchTableRepository).ToArray();

            ExportJobErrorService exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError, _repositoryFactory);
            exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
        }

        protected string GetImportApiSettingsForUser(Job job, string originalImportApiSettings)
        {
            LogGetImportApiSettingsForUserStart(job);

            ImportSettings importSettings = Serializer.Deserialize<ImportSettings>(originalImportApiSettings);
            importSettings.CorrelationId = ImportSettings.CorrelationId;
            importSettings.JobID = ImportSettings.JobID;
            importSettings.Provider = nameof(ProviderType.Relativity);
            AdjustImportApiSettings(job, importSettings);
            string serializedSettings = Serializer.Serialize(importSettings);

            GetImportApiSettingsForUserSuccessfulEnd(job, serializedSettings);
            return serializedSettings;
        }

        private void AdjustImportApiSettings(Job job, ImportSettings importSettings)
        {
            importSettings.OnBehalfOfUserId = job.SubmittedBy;
            importSettings.FederatedInstanceCredentials = IntegrationPointDto.SecuredConfiguration;
            SetImportAuditLevel(importSettings);
            SetOverwriteModeAccordingToUsersChoice(importSettings);

            bool shouldUseDgPaths = ShouldUseDgPaths(IntegrationPointDto.FieldMappings, SourceConfiguration);
            Logger.LogInformation("Should use DataGrid Paths set to {shouldUseDgPath}", shouldUseDgPaths);
            importSettings.LoadImportedFullTextFromServer = shouldUseDgPaths;
        }

        private void SetImportAuditLevel(ImportSettings importSettings)
        {
            if (importSettings.FederatedInstanceArtifactId.HasValue)
            {
                throw new NotSupportedException("i2i is not supported");
            }
        }

        private void SetOverwriteModeAccordingToUsersChoice(ImportSettings importSettings)
        {
            if (JobHistory != null)
            {
                Logger.LogInformation($@"Overwrite mode set to: {JobHistory.Overwrite}");
                importSettings.ImportOverwriteMode = NameToEnumConvert.GetEnumByModeName(JobHistory.Overwrite);
            }
        }

        private bool ShouldUseDgPaths(List<FieldMap> fieldMap, SourceConfiguration configuration)
        {
            IQueryFieldLookupRepository sourceQueryFieldLookupRepository =
                _repositoryFactory.GetQueryFieldLookupRepository(configuration.SourceWorkspaceArtifactId);
            IQueryFieldLookupRepository destinationQueryFieldLookupRepository =
                _repositoryFactory.GetQueryFieldLookupRepository(configuration.TargetWorkspaceArtifactId);

            FieldMap longTextField = fieldMap.FirstOrDefault(fm => IsLongTextWithDgEnabled(sourceQueryFieldLookupRepository.GetFieldByArtifactID(int.Parse(fm.SourceField.FieldIdentifier))));

            if (longTextField?.DestinationField?.FieldIdentifier != null && IsSingleDataGridField(fieldMap, sourceQueryFieldLookupRepository))
            {
                ViewFieldInfo destinationField = destinationQueryFieldLookupRepository.GetFieldByArtifactID(int.Parse(longTextField.DestinationField.FieldIdentifier));
                return destinationField != null && !destinationField.EnableDataGrid;
            }

            return false;
        }

        private bool IsLongTextWithDgEnabled(ViewFieldInfo info)
        {
            return info.Category == FieldCategory.FullText && info.EnableDataGrid;
        }

        private bool IsSingleDataGridField(IEnumerable<FieldMap> fieldMap, IQueryFieldLookupRepository source)
        {
            return fieldMap.Count(field => source.GetFieldByArtifactID(int.Parse(field.SourceField.FieldIdentifier)).EnableDataGrid) == 1;
        }

        protected override void JobHistoryErrorManagerSetup(Job job)
        {
            LogJobHistoryErrorManagerSetupStart(job);
            // Load Job History Errors if any
            string uniqueJobId = GetUniqueJobId(job);
            JobHistoryErrorManager =
                ManagerFactory.CreateJobHistoryErrorManager(SourceConfiguration.SourceWorkspaceArtifactId,
                    uniqueJobId);
            UpdateStatusType = JobHistoryErrorManager.StageForUpdatingErrors(job, JobHistory.JobType);

            if (SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
            {
                _sourceSavedSearchArtifactID = RetrieveSavedSearchArtifactId(job);

                // Load saved search for just item-level error retries
                if (UpdateStatusType.IsItemLevelErrorRetry())
                {
                    Logger.LogInformation("Creating item level errors saved search for retry job.");
                    _itemLevelErrorSavedSearchArtifactID = JobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job,
                        SourceConfiguration.SavedSearchArtifactId);
                    JobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, _itemLevelErrorSavedSearchArtifactID.Value);
                }
            }
            LogJobHistoryErrorManagerSetupSuccessfulEnd(job);
        }

        private int RetrieveSavedSearchArtifactId(Job job)
        {
            // Quick check to see if saved search is still available before using it for the job
            ISavedSearchQueryRepository savedSearchRepository =
                _repositoryFactory.GetSavedSearchQueryRepository(SourceConfiguration.SourceWorkspaceArtifactId);

            SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch(SourceConfiguration.SavedSearchArtifactId);
            if (savedSearch == null)
            {
                LogSavedSearchNotFound(job, SourceConfiguration);
                throw new IntegrationPointsException(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
            }
            return SourceConfiguration.SavedSearchArtifactId;
        }

        private void FinalizeExportServiceObservers(Job job)
        {
            LogFinalizeExportServiceObserversStart(job);
            SetJobStateAsUnstoppableIfNeeded(job);

            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.ForEach(_exportServiceJobObservers, jobObserver =>
            {
                try
                {
                    jobObserver.OnJobComplete(job);
                }
                catch (Exception exception)
                {
                    exceptions.Enqueue(exception);
                }
            });
            LogProblemsInFinalizeExportServiceObservers(job, exceptions);
            ThrowNewExceptionIfAny(exceptions);
            LogFinalizeExportServiceObserversSuccessfulEnd(job);
        }

        private void InitializeExportServiceObservers(Job job, string userImportApiSettings)
        {
            LogInitializeExportServiceObserversStart(job);
            ITagsCreator tagsCreator = ManagerFactory.CreateTagsCreator();
            ITagSavedSearchManager tagSavedSearchManager = ManagerFactory.CreateTaggingSavedSearchManager();
            ISourceWorkspaceTagCreator sourceWorkspaceTagsCreator = ManagerFactory.CreateSourceWorkspaceTagsCreator(SourceConfiguration);

            _exportServiceJobObservers = _exportServiceObserversFactory.InitializeExportServiceJobObservers(
                job,
                tagsCreator,
                tagSavedSearchManager,
                SynchronizerFactory,
                Serializer,
                JobHistoryErrorManager,
                JobStopManager,
                sourceWorkspaceTagsCreator,
                IntegrationPointDto.FieldMappings.ToArray(),
                SourceConfiguration,
                UpdateStatusType,
                JobHistory,
                GetUniqueJobId(job),
                userImportApiSettings);

            var exceptions = new ConcurrentQueue<Exception>();
            _exportServiceJobObservers.ForEach(batch =>
            {
                try
                {
                    batch.OnJobStart(job);
                }
                catch (Exception exception)
                {
                    exceptions.Enqueue(exception);
                }
            });

            LogProblemsInInitializeExportServiceObservers(job, exceptions);
            ThrowNewExceptionIfAny(exceptions);
            LogInitializeExportServiceObserversSuccessfulEnd(job);
        }

        private void FinalizeExportService(Job job)
        {
            LogFinalizeExportServiceStart(job);

            DeleteTempSavedSearch();

            LogFinalizeExportServiceSuccessfulEnd(job);
        }

        private void DeleteTempSavedSearch()
        {
            try
            {
                // we can delete the temp saved search (only gets called on retry for item-level only errors)
                if (UpdateStatusType == null || !UpdateStatusType.IsItemLevelErrorRetry())
                {
                    return;
                }
                if (!_itemLevelErrorSavedSearchArtifactID.HasValue)
                {
                    throw new InvalidOperationException(
                        "Item level error saved search has not been created, so it cannot be deleted.");
                }

                IJobHistoryErrorRepository jobHistoryErrorRepository =
                    _repositoryFactory.GetJobHistoryErrorRepository(SourceConfiguration.SourceWorkspaceArtifactId);
                jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_itemLevelErrorSavedSearchArtifactID.Value);
            }
            catch (Exception e)
            {
                LogDeletingTempSavedSearchError(e, SourceConfiguration);
                // IGNORE
            }
        }

        #region Logging

        private void LogGetImportApiSettingsForUserStart(Job job)
        {
            Logger.LogInformation("Getting Import API settings for user in job: {JobId}", job.JobId);
        }

        protected void LogProblemsInInitializeExportServiceObservers(Job job, IEnumerable<Exception> exceptions)
        {
            foreach (Exception ex in exceptions)
            {
                Logger.LogError(ex, "There was a problem while initializing export service observers {JobId}.", job.JobId);
            }
        }

        protected void LogProblemsInFinalizeExportServiceObservers(Job job, IEnumerable<Exception> exceptions)
        {
            foreach (Exception ex in exceptions)
            {
                Logger.LogError(ex, "There was a problem while finalizing export service observers {JobId}.", job.JobId);
            }
        }

        private void LogSavedSearchNotFound(Job job, SourceConfiguration sourceConfiguration)
        {
            Logger.LogError("Failed to retrieve Saved Search {SavedSearchArtifactId} for job {JobId}.",
                sourceConfiguration?.SavedSearchArtifactId, job.JobId);
        }

        private void LogDeletingTempSavedSearchError(Exception e, SourceConfiguration sourceConfiguration)
        {
            Logger.LogError(e, "Failed to delete temp Saved Search {SavedSearchArtifactId}.",
                sourceConfiguration?.SavedSearchArtifactId);
        }

        private void LogPushingDocumentsSuccessfulEnd(Job job)
        {
            Logger.LogInformation("Successfully finished pushing documents in Export Service Manager for job: {JobId}.",
                job.JobId);
        }

        private void LogPushingDocumentsStart(Job job)
        {
            Logger.LogInformation("Starting pushing documents in Export Service Manager for job: {JobId}.", job.JobId);
        }

        private void LogExecuteStart(Job job)
        {
            Logger.LogInformation("Started execution of job in Export Service Manager for job: {JobId}.", job.JobId);
        }

        private void LogExecuteEnd(Job job)
        {
            Logger.LogInformation("Finished execution of job in Export Service Manager for job: {JobId}.", job.JobId);
        }

        private void GetImportApiSettingsForUserSuccessfulEnd(Job job, string jsonString)
        {
            Logger.LogInformation("Successfully finished getting Import API settings for user. job: {JobId}", job.JobId);
            Logger.LogInformation("Getting Import API settings for user returned following json {jsonString}, job: {JobId} ",
                jsonString, job.JobId);
        }

        private void LogJobHistoryErrorManagerSetupStart(Job job)
        {
            Logger.LogInformation("Setting up Job History Error Manager for job: {JobId}", job.JobId);
        }

        private void LogJobHistoryErrorManagerSetupSuccessfulEnd(Job job)
        {
            Logger.LogInformation("Successfully finished Setting up Job History Error Manager for job: {JobId}", job.JobId);
        }

        private void LogFinalizeExportServiceObserversStart(Job job)
        {
            Logger.LogInformation("Finalizing export service observers for job: {JobId}", job.JobId);
        }

        private void LogFinalizeExportServiceObserversSuccessfulEnd(Job job)
        {
            Logger.LogInformation("Successfully finalized export service observers for job: {JobId}", job.JobId);
        }

        private void LogInitializeExportServiceObserversStart(Job job)
        {
            Logger.LogInformation("Initializing export service observers for job: {JobId}", job.JobId);
        }

        private void LogInitializeExportServiceObserversSuccessfulEnd(Job job)
        {
            Logger.LogInformation("Initialized export service observers for job: {JobId}", job.JobId);
        }

        private void LogFinalizeExportServiceSuccessfulEnd(Job job)
        {
            Logger.LogInformation("Finalized export service for job: {Job}", job);
        }

        private void LogFinalizeExportServiceStart(Job job)
        {
            Logger.LogInformation("Finalizing export service for job: {Job}", job);
        }

        #endregion
    }
}
