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
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
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
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
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
		private readonly ITempDocumentTableFactory _tempDocumentTableFactory;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly JobStatisticsService _statisticsService;
		private readonly List<IBatchStatus> _batchStatus;
		private readonly TaskResult _taskResult;
		private SourceConfiguration _sourceConfiguration;

		public ExportServiceManager(IHelper helper,
			ICaseServiceContext caseServiceContext,
			IContextContainerFactory contextContainerFactory,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			ITempDocumentTableFactory tempDocumentTableFactory,
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
			_tempDocumentTableFactory = tempDocumentTableFactory;
		}

		public IntegrationPoint IntegrationPointDto { get; private set; }
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

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				string userImportApiSettings = GetImportApiSettingsForUser(job, destinationConfig);
				IDataSynchronizer synchronizer = CreateDestinationProvider(destinationConfig);

				InitializeExportServiceObservers(job, userImportApiSettings);
				SetupSubscriptions(synchronizer, job);

				// Push documents
				using (IExporterService exporter = _exporterFactory.BuildExporter(MappedFields.ToArray(),
					IntegrationPointDto.SourceConfiguration,
					job.SubmittedBy))
				{
					JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
					UpdateJobStatus();

					if (exporter.TotalRecordsFound > 0)
					{
						IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IConsumeScratchTableBatchStatus>()
							.Select(observer => observer.ScratchTableRepository).ToArray();

						IDataReader dataReader = exporter.GetDataReader(scratchTables);
						synchronizer.SyncData(dataReader, MappedFields, userImportApiSettings);
					}
				}

				// tag documents
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

			_exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError);

			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
			_exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void FinalizeExportServiceObservers(Job job)
		{
			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_exportServiceJobObservers, batch =>
			{
				try
				{
					batch.JobComplete(job);
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
					batch.JobStarted(job);
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

			CheckForOtherJobsExecuting(job, this.JobHistoryDto.ArtifactId);

			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			UpdateJobStatus();

			_batchStatus.ForEach(batch => batch.JobStarted(job));
		}

		private void FinalizeExportService(Job job)
		{
			try
			{
				_exportServiceJobObservers.OfType<IScratchTableRepository>().ForEach(observer => observer.Dispose());
			}
			catch (Exception)
			{
				// trying to delete temp tables early, don't have worry about failing
			}

			foreach (IBatchStatus completedItem in _batchStatus)
			{
				try
				{
					completedItem.JobComplete(job);
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

			try
			{
				IntegrationPointDto.LastRuntimeUTC = DateTime.UtcNow;
				if (job.SerializedScheduleRule != null)
				{
					if (_taskResult.Status == TaskStatusEnum.None)
					{
						_taskResult.Status = TaskStatusEnum.Success;
					}
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
			string tempTableName = $"{job.JobId}_{_identifier}";
			ITempDocTableHelper docTableHelper = _tempDocumentTableFactory.GetDocTableHelper(tempTableName, _sourceConfiguration.SourceWorkspaceArtifactId);
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(_sourceConfiguration.SourceWorkspaceArtifactId);

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(docTableHelper,
				_sourceWorkspaceManager, _sourceJobManager,
				documentRepository, _synchronizerFactory,
				MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration,
				userImportApiSettings, JobHistoryDto.ArtifactId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTaggerDestinationWorkspace = new DestinationWorkspaceManager(_tempDocumentTableFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _sourceConfiguration, tempTableName, JobHistoryDto.ArtifactId, job.SubmittedBy);
			IConsumeScratchTableBatchStatus sourceJobHistoryTagger = new JobHistoryManager(_tempDocumentTableFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, JobHistoryDto.ArtifactId, _sourceConfiguration.SourceWorkspaceArtifactId, tempTableName, job.SubmittedBy);
			IBatchStatus sourceJobHistoryErrorUpdater = new JobHistoryErrorManager(_repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, JobHistoryDto.ArtifactId, _sourceConfiguration.SourceWorkspaceArtifactId, tempTableName, job.SubmittedBy);

			var batchStatusCommands = new List<IBatchStatus>()
			{
				destinationFieldsTagger,
				sourceFieldsTaggerDestinationWorkspace,
				sourceJobHistoryTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		private void CheckForOtherJobsExecuting(Job job, int jobHistoryId)
		{
			IQueueManager queueManager = _managerFactory.CreateQueueManager(_contextContainer);
			bool isScheduledJob = !String.IsNullOrEmpty(job.ScheduleRuleType);
			DateTime runTime = job.NextRunTime;

			bool hasJobsRunning = queueManager.HasJobsExecuting(_sourceConfiguration.SourceWorkspaceArtifactId, this.IntegrationPointDto.ArtifactId, job.JobId, runTime);

			if (hasJobsRunning)
			{
				string exceptionMessage = "Unable to execute Integration Point job: There is already a job currently running.";
				if (isScheduledJob)
				{
					this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult);
					exceptionMessage = $@"{exceptionMessage} The job has been re-scheduled for its next scheduled runtime.";
				}

				UnlinkJobHistoryFromIntegrationPoint(jobHistoryId);
				_jobHistoryService.DeleteRdo(jobHistoryId);

				throw new AgentDropJobException(exceptionMessage);
			}
		}

		private void UnlinkJobHistoryFromIntegrationPoint(int jobHistoryIdToRemove)
		{
			List<int> jobHistoryIds = this.IntegrationPointDto.JobHistory.ToList();
			int count = 0;
			foreach (int id in jobHistoryIds)
			{
				if (jobHistoryIdToRemove == id)
				{
					jobHistoryIds.RemoveAt(count);
					break;
				}
				count++;
			}
			this.IntegrationPointDto.JobHistory = jobHistoryIds.ToArray();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
		}
	}
}