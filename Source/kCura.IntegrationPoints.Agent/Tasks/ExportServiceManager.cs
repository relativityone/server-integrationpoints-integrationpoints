using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.DataReaderClient;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;
using APMClient = Relativity.Telemetry.APM.Client;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
	public class ExportServiceManager : ITask
	{
		private readonly List<IBatchStatus> _batchStatus;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IContextContainer _contextContainer;
		private readonly IExporterFactory _exporterFactory;
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;
		private readonly IAPILog _logger;
		private readonly IManagerFactory _managerFactory;
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;
		private readonly ISerializer _serializer;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly JobStatisticsService _statisticsService;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly TaskResult _taskResult;
		private ExportJobErrorService _exportJobErrorService;
		private List<IBatchStatus> _exportServiceJobObservers;
		private Guid _identifier;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private IJobStopManager _jobStopManager;
		private int _savedSearchArtifactId;
		private SourceConfiguration _sourceConfiguration;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;

		public ExportServiceManager(IHelper helper,
			ICaseServiceContext caseServiceContext,
			IContextContainerFactory contextContainerFactory,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			IRepositoryFactory repositoryFactory,
			IManagerFactory managerFactory,
			IEnumerable<IBatchStatus> statuses,
			ISerializer serializer,
			IJobService jobService,
			IScheduleRuleFactory scheduleRuleFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService)
		{
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
			_sourceJobManager = sourceJobManager;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_statisticsService = statisticsService;
			_synchronizerFactory = synchronizerFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceManager>();
			_taskResult = new TaskResult();
		}

		public IntegrationPoint IntegrationPointDto { get; private set; }
		public JobHistory JobHistoryDto { get; private set; }
		public List<FieldMap> MappedFields { get; private set; }
		public SourceProvider SourceProvider { get; private set; }

		public void Execute(Job job)
		{
			try
			{
				InitializeExportService(job);

				_jobStopManager.ThrowIfStopRequested();

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				string userImportApiSettings = GetImportApiSettingsForUser(job, destinationConfig);
				IDataSynchronizer synchronizer = CreateDestinationProvider(destinationConfig);

				try
				{
					_jobStopManager.ThrowIfStopRequested();

					InitializeExportServiceObservers(job, userImportApiSettings);
					SetupSubscriptions(synchronizer, job);

					_jobStopManager.ThrowIfStopRequested();

					// Push documents
					using (IExporterService exporter = _exporterFactory.BuildExporter(_jobStopManager, MappedFields.ToArray(),
						IntegrationPointDto.SourceConfiguration,
						_savedSearchArtifactId,
						job.SubmittedBy,
						userImportApiSettings))
					{
						lock (_jobStopManager.SyncRoot)
						{
							JobHistoryDto = _jobHistoryService.GetRdo(_identifier);
							JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
							UpdateJobStatus();
						}

						if (exporter.TotalRecordsFound > 0)
						{
							IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
								.Select(observer => observer.ScratchTableRepository).ToArray();

							IDataReader dataReader = exporter.GetDataReader(scratchTables);
							using (APMClient.APMClient.TimedOperation(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT))
							using (Client.MetricsClient.LogDuration(Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT,
								Guid.Empty))
							{
								synchronizer.SyncData(dataReader, MappedFields, userImportApiSettings);
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
				_taskResult.Status = TaskStatusEnum.Fail;
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				SetJobStateAsUnstoppable(job);
				_jobHistoryErrorService.CommitErrors();
				FinalizeExportService(job);
			}
		}

		private void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			IScratchTableRepository[] scratchTableToMonitorItemLevelError =
				_exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
					.Where(observer => observer.ScratchTableRepository.IgnoreErrorDocuments == false)
					.Select(observer => observer.ScratchTableRepository).ToArray();

			_exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError, _repositoryFactory);

			_statisticsService?.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
			_exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
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

		private void SetJobStateAsUnstoppable(Job job)
		{
			try
			{
				_jobStopManager?.Dispose();
				_jobService.UpdateStopState(new List<long> {job.JobId}, StopState.Unstoppable);
			}
			catch (Exception e)
			{
				LogSettingJobAsUnstoppableError(job, e);
				// Do not throw exception, we will need to dispose the rest of the objects.
			}
		}

		private void InitializeExportServiceObservers(Job job, string userImportApiSettings)
		{
			_exportServiceJobObservers = _exporterFactory.InitializeExportServiceJobObservers(job, _sourceWorkspaceManager,
				_sourceJobManager, _synchronizerFactory,
				_serializer, _jobHistoryErrorManager, _jobStopManager,
				MappedFields.ToArray(), _sourceConfiguration,
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

		private void InitializeExportService(Job job)
		{
			// Load integrationPoint data
			IntegrationPointDto = LoadIntegrationPointDto(job);
			_sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);
			_jobHistoryErrorService.IntegrationPoint = IntegrationPointDto;

			if (string.IsNullOrWhiteSpace(job.JobDetails))
			{
				TaskParameters taskParameters = new TaskParameters
				{
					BatchInstance = Guid.NewGuid()
				};
				_identifier = taskParameters.BatchInstance;
				job.JobDetails = _serializer.Serialize(taskParameters);
			}
			else
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				_identifier = taskParameters.BatchInstance;
			}

			JobHistoryDto = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(IntegrationPointDto, _identifier, DateTime.UtcNow);

			_jobHistoryErrorService.JobHistory = JobHistoryDto;
			JobHistoryDto.StartTimeUTC = DateTime.UtcNow;

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			UpdateJobStatus();

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = _serializer.Deserialize<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			//Load Job History Errors if any
			string uniqueJobId = GetUniqueJobId(job);
			_jobHistoryErrorManager = _managerFactory.CreateJobHistoryErrorManager(_contextContainer, _sourceConfiguration.SourceWorkspaceArtifactId, uniqueJobId);
			_updateStatusType = _jobHistoryErrorManager.StageForUpdatingErrors(job, JobHistoryDto.JobType);

			//Quick check to see if saved search is still available before using it for the job
			ISavedSearchQueryRepository savedSearchRepository =
				_repositoryFactory.GetSavedSearchQueryRepository(_sourceConfiguration.SourceWorkspaceArtifactId);

			SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch(_sourceConfiguration.SavedSearchArtifactId);
			if (savedSearch == null)
			{
				LogSavedSearchNotFound(job, _sourceConfiguration);
				throw new Exception(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
			}
			_savedSearchArtifactId = _sourceConfiguration.SavedSearchArtifactId;

			//Load saved search for just item-level error retries
			if (_updateStatusType.IsItemLevelErrorRetry())
			{
				_savedSearchArtifactId = _jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, _sourceConfiguration.SavedSearchArtifactId);
				_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, _savedSearchArtifactId);
			}

			_jobStopManager = _managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _identifier, job.JobId, true);
			_jobHistoryErrorService.JobStopManager = _jobStopManager;


			List<Exception> exceptions = new List<Exception>();
			_batchStatus.ForEach(batch =>
			{
				try
				{
					batch.OnJobStart(job);
				}
				catch (Exception exception)
				{
					_taskResult.Status = TaskStatusEnum.Fail;
					_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, exception);
					exceptions.Add(exception);
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

			foreach (IBatchStatus completedItem in _batchStatus)
			{
				try
				{
					completedItem.OnJobComplete(job);
				}
				catch (Exception e)
				{
					LogCompletingJobError(job, e, completedItem);
					_taskResult.Status = TaskStatusEnum.Fail;
					_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
				}
				finally
				{
					_jobHistoryErrorService.CommitErrors();
				}
			}

			if (_jobStopManager?.IsStopRequested() == true)
			{
				try
				{
					IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(_contextContainer);
					jobHistoryManager.SetErrorStatusesToExpired(_caseServiceContext.WorkspaceID, JobHistoryDto.ArtifactId);
				}
				catch (Exception e)
				{
					LogUpdatingStoppedJobStatusError(job, e);
					// ignore error. the status of errors will not affect the 'retry' nor the 'run' scenarios.
				}
			}
			UpdateIntegrationPointRuntimes(job);
		}

		private void UpdateIntegrationPointRuntimes(Job job)
		{
			try
			{
				IntegrationPointDto.LastRuntimeUTC = DateTime.UtcNow;
				if (job.SerializedScheduleRule != null)
				{
					if (_taskResult.Status == TaskStatusEnum.None)
					{
						_taskResult.Status = TaskStatusEnum.Success;
					}
					_jobService.UpdateStopState(new List<long> {job.JobId}, StopState.None);
					IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult);
				}
				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(IntegrationPointDto);
			}
			catch (Exception e)
			{
				LogUpdatingIntegrationPointRuntimesError(job, e);
			}
		}

		private void DeleteTempSavedSearch()
		{
			try
			{
				//we can delete the temp saved search (only gets called on retry for item-level only errors)
				if (_updateStatusType.IsItemLevelErrorRetry())
				{
					IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
					jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_savedSearchArtifactId);
				}
			}
			catch (Exception e)
			{
				LogDeletingTempSavedSearchError(e, _sourceConfiguration);
				// IGNORE
			}
		}

		private void FinalizeInProgressErrors(Job job)
		{
			// Finalize any In Progress Job History Errors
			if (_jobHistoryErrorService.JobLevelErrorOccurred)
			{
				JobHistoryErrorBatchUpdateManager sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory,
					_onBehalfOfUserClaimsPrincipalFactory, _jobStopManager, _sourceConfiguration.SourceWorkspaceArtifactId, job.SubmittedBy, _updateStatusType);
				sourceJobHistoryErrorUpdater.OnJobComplete(job);
			}
		}

		private IDataSynchronizer CreateDestinationProvider(string configuration)
		{
			// if you want to create add another synchronizer aka exporter, you may add it here.
			// RDO synchronizer
			GeneralWithCustodianRdoSynchronizerFactory factory = _synchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, configuration, IntegrationPointDto.SecuredConfiguration);
			return synchronizer;
		}

		private void UpdateJobStatus()
		{
			_jobHistoryService.UpdateRdo(JobHistoryDto);
		}

		private void ThrowNewExceptionIfAny(IEnumerable<Exception> exceptions)
		{
			if (exceptions != null)
			{
				Exception ex = null;
				Exception[] enumerable = exceptions as Exception[] ?? exceptions.ToArray();
				if (!enumerable.IsNullOrEmpty())
				{
					int counter = 0;
					string message = string.Join(Environment.NewLine,
						enumerable.Select(exception => $"{++counter}. {exception.Message}"));
					ex = new AggregateException(message, enumerable);
				}

				if (ex != null)
				{
					throw ex;
				}
			}
		}

		private string GetImportApiSettingsForUser(Job job, string originalImportApiSettings)
		{
			var importSettings = _serializer.Deserialize<ImportSettings>(originalImportApiSettings);
			importSettings.OnBehalfOfUserId = job.SubmittedBy;
			importSettings.FederatedInstanceCredentials = IntegrationPointDto.SecuredConfiguration;
			//Switch to Append/Overlay for error retries where original setting was Append Only
			if ((_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors) &&
				(importSettings.OverwriteMode == OverwriteModeEnum.Append))
			{
				importSettings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			}

			string jsonString = _serializer.Serialize(importSettings);
			return jsonString;
		}

		private IntegrationPoint LoadIntegrationPointDto(Job job)
		{
			int integrationPointId = job.RelatedObjectArtifactID;
			IntegrationPoint integrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);
			if (integrationPoint == null)
			{
				LogLoadingIntegrationPointDtoError(job);
				throw new ArgumentException("Failed to retrieve corresponding Integration Point.");
			}
			return integrationPoint;
		}

		private string GetUniqueJobId(Job job)
		{
			return job.JobId + "_" + _identifier;
		}

		#region Logging

		private void LogJobStoppedException(Job job, OperationCanceledException e)
		{
			_logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
		}

		private void LogExecutingTaskError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to execute ExportServiceManager task for job {JobId}.", job.JobId);
		}

		private void LogSettingJobAsUnstoppableError(Job job, Exception e)
		{
			_logger.LogError(e, "Failed to set job state as unstoppable for job {JobId}.", job.JobId);
		}

		private void LogSavedSearchNotFound(Job job, SourceConfiguration sourceConfiguration)
		{
			_logger.LogError("Failed to retrieve Saved Search {SavedSearchArtifactId} for job {JobId}.", sourceConfiguration?.SavedSearchArtifactId, job.JobId);
		}

		private void LogUpdatingStoppedJobStatusError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update job ({JobId}) status after job has been stopped.", job.JobId);
		}

		private void LogCompletingJobError(Job job, Exception exception, IBatchStatus batchStatus)
		{
			_logger.LogError(exception, "Failed to complete job {JobId}. Error occured in BatchStatus {BatchStatusType}.", job.JobId, batchStatus.GetType());
		}

		private void LogDisposingObserverError(Job job, Exception e)
		{
			_logger.LogError(e, "Failed to dispose observer for job {JobId}.", job.JobId);
		}

		private void LogUpdatingIntegrationPointRuntimesError(Job job, Exception e)
		{
			_logger.LogError(e, "Failed to update Integration Point runtimes for job {JobId}.", job.JobId);
		}

		private void LogDeletingTempSavedSearchError(Exception e, SourceConfiguration sourceConfiguration)
		{
			_logger.LogError(e, "Failed to delete temp Saved Search {SavedSearchArtifactId}.", sourceConfiguration?.SavedSearchArtifactId);
		}

		private void LogLoadingIntegrationPointDtoError(Job job)
		{
			_logger.LogError("Failed to retrieve corresponding Integration Point ({IntegrationPointId}) for job {JobId}.", job.RelatedObjectArtifactID, job.JobId);
		}

		#endregion
	}
}