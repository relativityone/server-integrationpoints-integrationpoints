using Castle.Core.Internal;
using kCura.IntegrationPoints.Agent.Attributes;
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
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.DataReaderClient;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
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
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
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

				_jobStopManager.ThrowIfStopRequested();

				InitializeExportServiceObservers(job, userImportApiSettings);
				SetupSubscriptions(synchronizer, job);

				_jobStopManager.ThrowIfStopRequested();
				// Push documents
				using (IExporterService exporter = _exporterFactory.BuildExporter(_jobStopManager, MappedFields.ToArray(),
					IntegrationPointDto.SourceConfiguration,
					_savedSearchArtifactId,
					job.SubmittedBy))
				{
					JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
					lock (_jobStopManager.SyncRoot)
					{
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
			catch (OperationCanceledException)
			{
				// ignore error.
			}
			catch (Exception ex)
			{
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
			IScratchTableRepository[] scratchTableToMonitorItemLevelError = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
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
				_jobService.UpdateStopState(new List<long> { job.JobId }, StopState.Unstoppable);
			}
			catch
			{
				// Do not throw exception, we will need to dispose the rest of the objects.
			}
		}

		private void InitializeExportServiceObservers(Job job, string userImportApiSettings)
		{
			_exportServiceJobObservers = _exporterFactory.InitializeExportServiceJobObservers( job, _sourceWorkspaceManager,
				_sourceJobManager, _synchronizerFactory, 
				_serializer, _jobHistoryErrorManager, _jobStopManager,
				MappedFields.ToArray(), _sourceConfiguration,
				_updateStatusType, IntegrationPointDto, JobHistoryDto,
				GetUniqueJobId(job), userImportApiSettings);

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

			this.JobHistoryDto = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(this.IntegrationPointDto, this._identifier, DateTime.UtcNow);

			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			UpdateJobStatus();

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = _serializer.Deserialize<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			//Load Job History Errors if any
			string uniqueJobId = GetUniqueJobId(job);
			_jobHistoryErrorManager = _managerFactory.CreateJobHistoryErrorManager(_contextContainer, _sourceConfiguration.SourceWorkspaceArtifactId, uniqueJobId);
			_updateStatusType = _jobHistoryErrorManager.StageForUpdatingErrors(job, this.JobHistoryDto.JobType);

			//Quick check to see if saved search is still available before using it for the job
			ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(_sourceConfiguration.SourceWorkspaceArtifactId, _sourceConfiguration.SavedSearchArtifactId);
			SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();
			if (savedSearch == null)
			{
				throw new Exception(Core.Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
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
				catch
				{
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
				catch
				{
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
					this._jobService.UpdateStopState(new List<long>() { job.JobId }, StopState.None);
					this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult);
				}
				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
			}
			catch
			{
				// ignored
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
			catch
			{
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
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, configuration);
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
					string message = String.Join(Environment.NewLine,
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

			//Switch to Append/Overlay for error retries where original setting was Append Only
			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors &&
				importSettings.OverwriteMode == OverwriteModeEnum.Append)
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
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}
			return integrationPoint;
		}

		private string GetUniqueJobId(Job job)
		{
			return job.JobId + "_" + this._identifier;
		}
	}
}