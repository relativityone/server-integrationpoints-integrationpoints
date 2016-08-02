using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.DataReaderClient;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ITask
	{
		private ExportJobErrorService _exportJobErrorService;
		private Guid _identifier;
		private List<IBatchStatus> _exportServiceJobObservers;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IContextContainer _contextContainer;
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private readonly IExporterFactory _exporterFactory;
		private readonly IJobService _jobService;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly JobStatisticsService _statisticsService;
		private readonly List<IBatchStatus> _batchStatus;
		private readonly TaskResult _taskResult;
		private SourceConfiguration _sourceConfiguration;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private int _savedSearchArtifactId;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private IJobStopManager _jobStopManager;

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
			Apps.Common.Utils.Serializers.ISerializer serializer,
			IJobService jobService,
			IScheduleRuleFactory scheduleRuleFactory,
			IJobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
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
			_taskResult = new TaskResult();
		}

		public Data.IntegrationPoint IntegrationPointDto { get; private set; }
		public JobHistory JobHistoryDto { get; private set; }
		public JobHistoryError JobHistoryErrorDto { get; private set; }
		public List<FieldMap> MappedFields { get; private set; }
		public SourceProvider SourceProvider { get; private set; }

		public void Execute(Job job)
		{
			bool agentDroppedJob = false;
			try
			{
				InitializeExportService(job);

				if(_jobStopManager.IsStoppingRequested()) { return; }

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				string userImportApiSettings = GetImportApiSettingsForUser(job, destinationConfig);
				IDataSynchronizer synchronizer = CreateDestinationProvider(destinationConfig);

				if (_jobStopManager.IsStoppingRequested()) { return; }

				InitializeExportServiceObservers(job, userImportApiSettings);
				SetupSubscriptions(synchronizer, job);

				if (_jobStopManager.IsStoppingRequested()) { return; }

				// Push documents
				using (IExporterService exporter = _exporterFactory.BuildExporter(_jobStopManager, MappedFields.ToArray(),
					IntegrationPointDto.SourceConfiguration,
					_savedSearchArtifactId,
					job.SubmittedBy))
				{
					JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
					lock (_jobStopManager.SyncRoot)
					{
						if (!_jobStopManager.IsStoppingRequested())
						{
							JobHistoryDto.JobStatus = JobStatusChoices.JobHistoryProcessing;
						}
						UpdateJobStatus();
					}

					if (exporter.TotalRecordsFound > 0)
					{
						IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
							.Select(observer => observer.ScratchTableRepository).ToArray();

						IDataReader dataReader = exporter.GetDataReader(scratchTables);
						synchronizer.SyncData(dataReader, MappedFields, userImportApiSettings);
					}
				}

				FinalizeExportServiceObservers(job);
			}
			catch (AgentDropJobException) //for concurrency, an Agent drops a job if one is already executing
			{
				agentDroppedJob = true;
				throw;
			}
			catch (Exception ex)
			{
				_taskResult.Status = TaskStatusEnum.Fail;
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobStopManager.Dispose();
				if (!agentDroppedJob)
				{
					_jobHistoryErrorService.CommitErrors();
					FinalizeExportService(job);
				}
			}
		}

		private void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			IScratchTableRepository[] scratchTableToMonitorItemLevelError = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
				.Where(observer => observer.ScratchTableRepository.IgnoreErrorDocuments == false)
				.Select(observer => observer.ScratchTableRepository).ToArray();

			_exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError, _repositoryFactory);

			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
			_exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void FinalizeExportServiceObservers(Job job)
		{
			_jobStopManager.Dispose();
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
			_exportServiceJobObservers = InitializeExportServiceJobObservers(job, userImportApiSettings);

			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_exportServiceJobObservers, batch =>
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
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;

			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			this._identifier = taskParameters.BatchInstance;

			this.JobHistoryDto = _jobHistoryService.CreateRdo(this.IntegrationPointDto, this._identifier, DateTime.UtcNow);
			_jobHistoryService.GetRdo(_identifier);

			CheckForOtherJobsExecuting(job, this.JobHistoryDto.ArtifactId);

			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			UpdateJobStatus();

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			//Load Job History Errors if any
			string uniqueJobId = GetUniqueJobId(job);
			_jobHistoryErrorManager = _managerFactory.CreateJobHistoryErrorManager(_contextContainer, _sourceConfiguration.SourceWorkspaceArtifactId, uniqueJobId);
			_updateStatusType = _jobHistoryErrorManager.StageForUpdatingErrors(job, this.JobHistoryDto.JobType);

			ExportUsingSavedSearchSettings exportSettings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(IntegrationPointDto.SourceConfiguration);
			//Quick check to see if saved search is still available before using it for the job
			ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(_sourceConfiguration.SourceWorkspaceArtifactId, exportSettings.SavedSearchArtifactId);
			SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();
			if (savedSearch == null)
			{
				throw new Exception(Core.Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
			}
			_savedSearchArtifactId = exportSettings.SavedSearchArtifactId;

			//Load saved search for just item-level error retries
			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors &&
			    _updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
			{
				_savedSearchArtifactId = _jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, exportSettings.SavedSearchArtifactId);
				_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, _savedSearchArtifactId);
			}

			_jobStopManager = _managerFactory.CreateJobStopManager(null, _jobService, _jobHistoryService, _identifier, job.JobId);
			_jobHistoryErrorService.StopJobStopManager = _jobStopManager;

			_batchStatus.ForEach(batch => batch.OnJobStart(job));
		}
		
		private void FinalizeExportService(Job job)
		{
			try
			{
				_exportServiceJobObservers.OfType<IScratchTableRepository>().ForEach(observer => observer.Dispose());

				//Now we can delete the temp saved search (only gets called on retry for item-level only errors)
				if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors &&
					_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
				{
					IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
					jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_savedSearchArtifactId);
				}

				// Finalize any In Progress Job History Errors
				if (_jobHistoryErrorService.JobLevelErrorOccurred)
				{
					JobHistoryErrorBatchUpdateManager sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory,
							_onBehalfOfUserClaimsPrincipalFactory, _sourceConfiguration.SourceWorkspaceArtifactId, job.SubmittedBy, _updateStatusType);
					sourceJobHistoryErrorUpdater.OnJobComplete(job);
				}
			}

			catch (Exception)
			{
				// trying to delete temp tables early, don't have worry about failing
			}

			foreach (IBatchStatus completedItem in _batchStatus)
			{
				try
				{
					completedItem.OnJobComplete(job);
				}
				catch (Exception e)
				{
					_taskResult.Status = TaskStatusEnum.Fail;
					_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
				}
				finally
				{
					_jobHistoryErrorService.CommitErrors();
				}
			}


			if (_jobStopManager.IsStoppingRequested())
			{
				try
				{
					IJobHistoryRepository jobHistoryRepo = _repositoryFactory.GetJobHistoryRepository(_caseServiceContext.WorkspaceID);
					jobHistoryRepo.SetErrorStatusesToExpired(JobHistoryDto.ArtifactId);
				}
				catch { }
			}

			try
			{
				IntegrationPointDto.LastRuntimeUTC = DateTime.UtcNow;
				if (job.SerializedScheduleRule != null)
				{
					if (_taskResult.Status == TaskStatusEnum.None)
					{
						_taskResult.Status = TaskStatusEnum.Success;
					}
					this._jobService.UpdateStopState(new List<long>() { job.JobId }, StopState.None );
					this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult); //use this for concurrency -MNG
				}

				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
			}
			catch
			{
				// ignored
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
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, configuration);
			return synchronizer;
		}

		private void UpdateJobStatus()
		{
			_jobHistoryService.UpdateRdo(JobHistoryDto);
		}

		private void ThrowNewExceptionIfAny(IEnumerable<Exception> exceptions)
		{
			if (exceptions == null)
			{
				return;
			}

			Exception ex = null;
			Exception[] enumerable = exceptions as Exception[] ?? exceptions.ToArray();
			if (!enumerable.IsNullOrEmpty())
			{
				int counter = 0;
				string message = String.Join(Environment.NewLine, enumerable.Select(exception => $"{++counter}. {exception.Message}"));
				ex = new AggregateException(message, enumerable);
			}

			if (ex != null)
			{
				throw ex;
			}
		}

		private string GetImportApiSettingsForUser(Job job, string originalImportApiSettings)
		{
			var importSettings = JsonConvert.DeserializeObject<ImportSettings>(originalImportApiSettings);
			importSettings.OnBehalfOfUserId = job.SubmittedBy;

			//Switch to Append/Overlay for error retries where original setting was Append Only
			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors &&
			    importSettings.OverwriteMode == OverwriteModeEnum.Append)
			{
				importSettings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			}

			string jsonString = JsonConvert.SerializeObject(importSettings);
			return jsonString;
		}

		private IntegrationPoint LoadIntegrationPointDto(Job job)
		{
			int integrationPointId = job.RelatedObjectArtifactID;
			IntegrationPoint integrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);

			if (integrationPoint == null)
			{
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}
			return integrationPoint;
		}

		private List<IBatchStatus> InitializeExportServiceJobObservers(Job job, string userImportApiSettings)
		{
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
			string uniqueJobId = GetUniqueJobId(job);

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_repositoryFactory, _sourceWorkspaceManager, 
				_sourceJobManager, documentRepository, _synchronizerFactory, MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration, 
				userImportApiSettings, JobHistoryDto.ArtifactId, uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTagger = new SourceObjectBatchUpdateManager(_repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _sourceConfiguration, JobHistoryDto.ArtifactId, job.SubmittedBy, uniqueJobId);
			IBatchStatus sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _sourceConfiguration.SourceWorkspaceArtifactId, job.SubmittedBy, _updateStatusType);

			var batchStatusCommands = new List<IBatchStatus>()
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		private void CheckForOtherJobsExecuting(Job job, int jobHistoryId)
		{
			IQueueManager queueManager = _managerFactory.CreateQueueManager(_contextContainer);
			DateTime runTime = job.NextRunTime;

			bool hasExecutingJobs = queueManager.HasJobsExecuting(_sourceConfiguration.SourceWorkspaceArtifactId, this.IntegrationPointDto.ArtifactId, job.JobId, runTime);

			if (hasExecutingJobs)
			{
				DropJobAndCleanupJobHistory(job, jobHistoryId);
			}
		}

		private void DropJobAndCleanupJobHistory(Job job, int jobHistoryId)
		{
			string exceptionMessage = "Unable to execute Integration Point job: There is already a job currently running.";
			//check if it's a scheduled job
			if (!String.IsNullOrEmpty(job.ScheduleRuleType))
			{
				this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult);
				exceptionMessage = $@"{exceptionMessage} Job is re-scheduled for {this.IntegrationPointDto.NextScheduledRuntimeUTC}.";
			}

			UnlinkJobHistoryFromIntegrationPoint(jobHistoryId);
			_jobHistoryService.DeleteRdo(jobHistoryId);

			throw new AgentDropJobException(exceptionMessage);
		}

		private void UnlinkJobHistoryFromIntegrationPoint(int jobHistoryIdToRemove)
		{
			List<int> jobHistoryIds = this.IntegrationPointDto.JobHistory.ToList();
			jobHistoryIds.Remove(jobHistoryIdToRemove);
			this.IntegrationPointDto.JobHistory = jobHistoryIds.ToArray();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
		}

		private string GetUniqueJobId(Job job)
		{
			return job.JobId + "_" + this._identifier;
		}
	}
}