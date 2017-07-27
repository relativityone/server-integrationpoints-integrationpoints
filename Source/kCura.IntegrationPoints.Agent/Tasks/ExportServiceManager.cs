using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.DataReaderClient;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;
using Relativity.Telemetry.MetricsCollection;
using APMClient = Relativity.Telemetry.APM.Client;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ServiceManagerBase
	{
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IExporterFactory _exporterFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private ExportJobErrorService _exportJobErrorService;
		private List<IBatchStatus> _exportServiceJobObservers;
		private int _savedSearchArtifactId;
		private IJobHistoryErrorManager _jobHistoryErrorManager { get; set; }
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType { get; set; }

		public ExportServiceManager(IHelper helper,
			IHelperFactory helperFactory,
			ICaseServiceContext caseServiceContext,
			IContextContainerFactory contextContainerFactory,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory,
			IRepositoryFactory repositoryFactory,
			IManagerFactory managerFactory,
			IEnumerable<IBatchStatus> statuses,
			ISerializer serializer,
			IJobService jobService,
			IScheduleRuleFactory scheduleRuleFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService)
			: base(helper,
				  jobService,
				  serializer,
				  jobHistoryService,
				  jobHistoryErrorService,
				  scheduleRuleFactory,
				  managerFactory,
				  contextContainerFactory,
				  statuses,
				  caseServiceContext,
				  onBehalfOfUserClaimsPrincipalFactory,
				  statisticsService,
				  synchronizerFactory)
		{

			_contextContainerFactory = contextContainerFactory;
			_repositoryFactory = repositoryFactory;
			_exporterFactory = exporterFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			Logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceManager>();
		}

		public override void Execute(Job job)
		{
			try
			{
			    LogExecuteStart(job);
                InitializeService(job);

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

					// Push documents
					using (IExporterService exporter = _exporterFactory.BuildExporter(JobStopManager, MappedFields.ToArray(),
						IntegrationPointDto.SourceConfiguration,
						_savedSearchArtifactId,
						job.SubmittedBy,
						userImportApiSettings))
					{
					    LogPushingDocumentsStart(job);
                        IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
								.Select(observer => observer.ScratchTableRepository).ToArray();

						var exporterTransferConfiguration = new ExporterTransferConfiguration(scratchTables, JobHistoryService, Identifier, Serializer.Deserialize<ImportSettings>(userImportApiSettings));
						IDataTransferContext dataTransferContext = exporter.GetDataTransferContext(exporterTransferConfiguration);

						lock (JobStopManager.SyncRoot)
						{
							JobHistoryDto = JobHistoryService.GetRdo(Identifier);
							dataTransferContext.UpdateTransferStatus();
						}

						if (exporter.TotalRecordsFound > 0)
						{

							using (APMClient.APMClient.TimedOperation(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT))
							using (Client.MetricsClient.LogDuration(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT,
								Guid.Empty))
							{
								synchronizer.SyncData(dataTransferContext, MappedFields, userImportApiSettings);
							}
						}
					    LogPushingDocumetsSuccesfulEnd(job);
                    }
				}
				finally
				{
					using (APMClient.APMClient.TimedOperation(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_TARGET_DOCUMENTS_TAGGING_IMPORT))
					using (Client.MetricsClient.LogDuration(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_TARGET_DOCUMENTS_TAGGING_IMPORT,
						Guid.Empty))
					{
						FinalizeExportServiceObservers(job);
					}
				}
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				// ignore error.
			}
			catch (Exception ex)
			{
				LogExecutingTaskError(job, ex);
				Result.Status = TaskStatusEnum.Fail;
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				SetJobStateAsUnstoppable(job);
				JobHistoryErrorService.CommitErrors();
				FinalizeExportService(job);
				FinalizeService(job);
			    LogExecuteEnd(job);
            }
		}




	    protected override void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			IScratchTableRepository[] scratchTableToMonitorItemLevelError =
				_exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
					.Where(observer => observer.ScratchTableRepository.IgnoreErrorDocuments == false)
					.Select(observer => observer.ScratchTableRepository).ToArray();

			_exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError, _repositoryFactory);

			StatisticsService?.Subscribe(synchronizer as IBatchReporter, job);
			JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
			_exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		protected string GetImportApiSettingsForUser(Job job, string originalImportApiSettings)
		{
		    LogGetImportApiSettingsForUserStart(job);

            var importSettings = Serializer.Deserialize<ImportSettings>(originalImportApiSettings);
			importSettings.OnBehalfOfUserId = job.SubmittedBy;
			importSettings.FederatedInstanceCredentials = IntegrationPointDto.SecuredConfiguration;

			if (importSettings.FederatedInstanceArtifactId.HasValue)
			{
				var targetHelper = _helperFactory.CreateTargetHelper(_helper, importSettings.FederatedInstanceArtifactId, importSettings.FederatedInstanceCredentials);
				var contextContainer = ContextContainerFactory.CreateContextContainer(targetHelper);
				var instanceSettingsManager = ManagerFactory.CreateInstanceSettingsManager(contextContainer);

				if (!instanceSettingsManager.RetrieveAllowNoSnapshotImport())
				{
					importSettings.ImportAuditLevel = ImportAuditLevelEnum.FullAudit;
				}
			}

			//Switch to Append/Overlay for error retries where original setting was Append Only
			if ((_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors) &&
				(importSettings.OverwriteMode == OverwriteModeEnum.Append))
			{
				importSettings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			}

			string jsonString = Serializer.Serialize(importSettings);
		    GetImportApiSettingsForUserSuccesfulEnd(job, jsonString);
            return jsonString;
		}

	    protected override void JobHistoryErrorManagerSetup(Job job)
		{
		    LogJobHistoryErrorManagerSetupStart(job);
            //Load Job History Errors if any
            string uniqueJobId = GetUniqueJobId(job);
			_jobHistoryErrorManager = ManagerFactory.CreateJobHistoryErrorManager(ContextContainer, SourceConfiguration.SourceWorkspaceArtifactId, uniqueJobId);
			_updateStatusType = _jobHistoryErrorManager.StageForUpdatingErrors(job, JobHistoryDto.JobType);

			if (SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
			{


				//Quick check to see if saved search is still available before using it for the job
				ISavedSearchQueryRepository savedSearchRepository =
					_repositoryFactory.GetSavedSearchQueryRepository(SourceConfiguration.SourceWorkspaceArtifactId);

				SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch(SourceConfiguration.SavedSearchArtifactId);
				if (savedSearch == null)
				{
					LogSavedSearchNotFound(job, SourceConfiguration);
					throw new Exception(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
				}
				_savedSearchArtifactId = SourceConfiguration.SavedSearchArtifactId;


				//Load saved search for just item-level error retries
				if (_updateStatusType.IsItemLevelErrorRetry())
				{
					_savedSearchArtifactId = _jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job,
						SourceConfiguration.SavedSearchArtifactId);
					_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, _savedSearchArtifactId);
				}
			}
		    LogJobHistoryErrorManagerSetupSuccesfulEnd(job);
        }

        private void FinalizeExportServiceObservers(Job job)
		{
		    LogFinalizeExportServiceObserversStart(job);
            SetJobStateAsUnstoppable(job);

			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_exportServiceJobObservers, batch =>
			{
				try
				{
					batch.OnJobComplete(job);
				}
				catch (Exception exception)
				{
					exceptions.Enqueue(exception);
				}
			});
            LogProblemsInFinalizeExportServiceObservers(job, exceptions);
			ThrowNewExceptionIfAny(exceptions);
		    LogFinalizeExportServiceObserversSuccesfulEnd(job);
        }

        private void InitializeExportServiceObservers(Job job, string userImportApiSettings)
		{
		    LogInitializeExportServiceObserversStart(job);
            var settings = Serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, settings.FederatedInstanceArtifactId, IntegrationPointDto.SecuredConfiguration);
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper,
				targetHelper.GetServicesManager());
			ITagsCreator tagsCreator = ManagerFactory.CreateTagsCreator(contextContainer);
			ITagSavedSearchManager tagSavedSearchManager = ManagerFactory.CreateTaggingSavedSearchManager(contextContainer);

			_exportServiceJobObservers = _exporterFactory.InitializeExportServiceJobObservers(job, tagsCreator, tagSavedSearchManager, SynchronizerFactory,
				Serializer, _jobHistoryErrorManager, JobStopManager,
				MappedFields.ToArray(), SourceConfiguration,
				_updateStatusType, IntegrationPointDto, JobHistoryDto,
				GetUniqueJobId(job), userImportApiSettings);

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
		    LogInitializeExportServiceObserversSuccesfulEnd(job);
        }

	    private void FinalizeExportService(Job job)
		{
		    LogFinalizeExportServiceStart(job);

            _exportServiceJobObservers?.OfType<IScratchTableRepository>().ForEach(observer =>
			{
				try
				{
					observer.Dispose();
				}
				catch (Exception e)
				{
					LogDisposingObserverError(job, e);
					// trying to delete temp tables early, don't have worry about failing
				}
			});

			DeleteTempSavedSearch();
			FinalizeInProgressErrors(job);
		    LogFinalizeExportServiceSuccesfulEnd(job);
		}


	    private void FinalizeInProgressErrors(Job job)
		{
            // Finalize any In Progress Job History Errors
            if (JobHistoryErrorService.JobLevelErrorOccurred)
            {
                LogFinalizeInProgressErrors(job);

                JobHistoryErrorBatchUpdateManager sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory,
					OnBehalfOfUserClaimsPrincipalFactory, JobStopManager, SourceConfiguration.SourceWorkspaceArtifactId, job.SubmittedBy, _updateStatusType);
				sourceJobHistoryErrorUpdater.OnJobComplete(job);
                LogFinalizeInProgressErrorsDone(job);
            }
		}

	    private void DeleteTempSavedSearch()
		{
			try
			{
				//we can delete the temp saved search (only gets called on retry for item-level only errors)
				if (_updateStatusType.IsItemLevelErrorRetry())
				{
					IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceConfiguration.SourceWorkspaceArtifactId);
					jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_savedSearchArtifactId);
				}
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

		protected void  LogProblemsInFinalizeExportServiceObservers(Job job, IEnumerable<Exception> exceptions)
		{
		    foreach (Exception ex in exceptions)
		    {
                Logger.LogError(ex, "There was a problem while finalizing export service observers {JobId}.", job.JobId);
		    }
		}

		protected override void LogExecutingTaskError(Job job, Exception ex)
		{
			Logger.LogError(ex, "Failed to execute ExportServiceManager task for job {JobId}.", job.JobId);
		}

		private void LogSavedSearchNotFound(Job job, SourceConfiguration sourceConfiguration)
		{
			Logger.LogError("Failed to retrieve Saved Search {SavedSearchArtifactId} for job {JobId}.", sourceConfiguration?.SavedSearchArtifactId, job.JobId);
		}

		private void LogDeletingTempSavedSearchError(Exception e, SourceConfiguration sourceConfiguration)
		{
			Logger.LogError(e, "Failed to delete temp Saved Search {SavedSearchArtifactId}.", sourceConfiguration?.SavedSearchArtifactId);
		}

	    private void LogPushingDocumetsSuccesfulEnd(Job job)
	    {
	        Logger.LogInformation("Succesfully finished pushing documents in Export Service Manager for job: {JobId}.", job.JobId);
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

	    private void GetImportApiSettingsForUserSuccesfulEnd(Job job, string jsonString)
	    {
	        Logger.LogInformation("Succesfully finished getting Import API settings for user. job: {JobId}", job.JobId);
	        Logger.LogDebug("Getting Import API settings for user returned following json {jsonString}, job: {JobId} ", jsonString, job.JobId);
	    }

	    private void LogJobHistoryErrorManagerSetupStart(Job job)
	    {
	        Logger.LogInformation("Setting up Job History Error Manager for job: {JobId}", job.JobId);
	    }

	    private void LogJobHistoryErrorManagerSetupSuccesfulEnd(Job job)
	    {
	        Logger.LogInformation("Succesfully finished Setting up Job History Error Manager for job: {JobId}", job.JobId);
	    }

	    private void LogFinalizeExportServiceObserversStart(Job job)
	    {
	        Logger.LogInformation("Finalizing export service observers for job: {JobId}", job.JobId);
	    }

	    private void LogFinalizeExportServiceObserversSuccesfulEnd(Job job)
	    {
	        Logger.LogInformation("Succesfully finalized export service observers for job: {JobId}", job.JobId);
	    }

	    private void LogInitializeExportServiceObserversStart(Job job)
	    {
	        Logger.LogInformation("Initializing export service observers for job: {JobId}", job.JobId);
	    }
	    private void LogInitializeExportServiceObserversSuccesfulEnd(Job job)
	    {
	        Logger.LogInformation("Initialized export service observers for job: {JobId}", job.JobId);
	    }

	    private void LogFinalizeExportServiceSuccesfulEnd(Job job)
	    {
	        Logger.LogInformation("Finalized export service for job: {Job}", job);
	    }

	    private void LogFinalizeExportServiceStart(Job job)
	    {
	        Logger.LogInformation("Finalizing export service for job: {Job}", job);
	    }

	    private void LogFinalizeInProgressErrorsDone(Job job)
	    {
	        Logger.LogInformation("Finished Finalizing job level errors for job: {JobId}", job.JobId);
	    }

	    private void LogFinalizeInProgressErrors(Job job)
	    {
	        Logger.LogInformation("Finalizing job level errors for job: {JobId}", job.JobId);
	    }
        #endregion
    }
}