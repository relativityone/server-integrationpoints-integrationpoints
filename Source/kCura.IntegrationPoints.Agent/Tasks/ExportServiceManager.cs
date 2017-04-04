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
		/*	
<<<<<<< HEAD

=======
			_batchStatus = statuses.ToList();
			_caseServiceContext = caseServiceContext;
			
			_contextContainer = contextContainerFactory.CreateContextContainer(helper);
			_exporterFactory = exporterFactory;
			_onBehalfOfUserClaimsPrincipalFactory = onBehalfOfUserClaimsPrincipalFactory;
			_jobHistoryErrorService = jobHistoryErrorService;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
			_repositoryFactory = repositoryFactory;
			_managerFactory = managerFactory;
			_scheduleRuleFactory = scheduleRuleFactory;
			_serializer = serializer;
			_statisticsService = statisticsService;
			_synchronizerFactory = synchronizerFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceManager>();
			_taskResult = new TaskResult();
>>>>>>> develop
*/
		}

		public override void Execute(Job job)
		{
			try
			{
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
						IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
								.Select(observer => observer.ScratchTableRepository).ToArray();

						var exporterTransferConfiguration = new ExporterTransferConfiguration(scratchTables, JobHistoryService, Identifier);
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
			return jsonString;
		}

		protected override void JobHistoryErrorManagerSetup(Job job)
		{
			//Load Job History Errors if any
			string uniqueJobId = GetUniqueJobId(job);
			_jobHistoryErrorManager = ManagerFactory.CreateJobHistoryErrorManager(ContextContainer, SourceConfiguration.SourceWorkspaceArtifactId, uniqueJobId);
			_updateStatusType = _jobHistoryErrorManager.StageForUpdatingErrors(job, JobHistoryDto.JobType);

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
				_savedSearchArtifactId = _jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, SourceConfiguration.SavedSearchArtifactId);
				_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, _savedSearchArtifactId);
			}
		}

		private void FinalizeExportServiceObservers(Job job)
		{
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
			ThrowNewExceptionIfAny(exceptions);
		}

		private void InitializeExportServiceObservers(Job job, string userImportApiSettings)
		{
			var settings = Serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, settings.FederatedInstanceArtifactId, IntegrationPointDto.SecuredConfiguration);
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper,
				targetHelper.GetServicesManager());
			ISourceWorkspaceManager sourceWorkspaceManager = ManagerFactory.CreateSourceWorkspaceManager(contextContainer);
			ISourceJobManager sourceJobManager = ManagerFactory.CreateSourceJobManager(contextContainer);

			_exportServiceJobObservers = _exporterFactory.InitializeExportServiceJobObservers(job, sourceWorkspaceManager,
				sourceJobManager, SynchronizerFactory,
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

			ThrowNewExceptionIfAny(exceptions);
		}

		private void FinalizeExportService(Job job)
		{
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
		}

		private void FinalizeInProgressErrors(Job job)
		{
			// Finalize any In Progress Job History Errors
			if (JobHistoryErrorService.JobLevelErrorOccurred)
			{
				JobHistoryErrorBatchUpdateManager sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory,
					OnBehalfOfUserClaimsPrincipalFactory, JobStopManager, SourceConfiguration.SourceWorkspaceArtifactId, job.SubmittedBy, _updateStatusType);
				sourceJobHistoryErrorUpdater.OnJobComplete(job);
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

		#endregion
	}
}