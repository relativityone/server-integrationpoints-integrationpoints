using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ITask
	{
		private ExportJobErrorService _exportJobErrorService;
		private Guid _identifier;
		private List<IBatchStatus> _exportServiceJobObservers;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IDocumentRepository _documentRepository;
		private readonly IExporterFactory _exporterFactory;
		private readonly IJobService _jobService;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly ITempDocumentTableFactory _tempDocumentTableFactory;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private readonly JobHistoryService _jobHistoryService;
		private readonly JobStatisticsService _statisticsService;
		private readonly List<IBatchStatus> _batchStatus;
		private readonly TaskResult _taskResult;
		private SourceConfiguration _sourceConfiguration;

		public ExportServiceManager(
			ICaseServiceContext caseServiceContext,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			ITempDocumentTableFactory tempDocumentTableFactory,
			IRepositoryFactory repositoryFactory,
			IEnumerable<IBatchStatus> statuses,
			IDocumentRepository documentRepository,
			Apps.Common.Utils.Serializers.ISerializer serializer,
			IJobService jobService,
			IScheduleRuleFactory scheduleRuleFactory,
			JobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService)
		{
			_batchStatus = statuses.ToList();
			_caseServiceContext = caseServiceContext;
			_documentRepository = documentRepository;
			_exporterFactory = exporterFactory;
			_jobHistoryErrorService = jobHistoryErrorService;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
			_repositoryFactory = repositoryFactory;
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
		public List<FieldMap> MappedFields { get; private set; }
		public SourceProvider SourceProvider { get; private set; }

		public void Execute(Job job)
		{
			try
			{
				InitializeExportService(job);

				InitializeExportServiceObservers(job);

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				IDataSynchronizer synchronizer = CreateDestinationProvider(destinationConfig);

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
						IScratchTableRepository[] scratchTables = _exportServiceJobObservers.OfType<IScratchTableRepository>().ToArray();
						IDataReader dataReader = exporter.GetDataReader(scratchTables);

						string newImportApiSettings = GetImportApiSettingsWithOnBehalfUserInformation(job, destinationConfig);
						synchronizer.SyncData(dataReader, MappedFields, newImportApiSettings);
					}
				}

				// tag documents
				FinalizeExportServiceObservers(job);
			}
			catch (Exception ex)
			{
				_taskResult.Status = TaskStatusEnum.Fail;
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				FinalizeExportService(job);
			}
		}

		private void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{

			IScratchTableRepository[] scratchTableToMonitorItemLevelError = _exportServiceJobObservers.OfType<IScratchTableRepository>()
				.Where(scratchTable => scratchTable.IgnoreErrorDocuments == false).ToArray();

			_exportJobErrorService = new ExportJobErrorService(scratchTableToMonitorItemLevelError);
			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;

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

		private void InitializeExportServiceObservers(Job job)
		{
			_exportServiceJobObservers = InitializeExportServiceJobObservers(job);

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
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			this._identifier = taskParameters.BatchInstance;

			// Load integrationPoint data
			IntegrationPointDto = LoadIntegrationPointDto(job);

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			_sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			this.JobHistoryDto = _jobHistoryService.CreateRdo(this.IntegrationPointDto, this._identifier, DateTime.UtcNow);
			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;
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
					this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, _taskResult);
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

		private string GetImportApiSettingsWithOnBehalfUserInformation(Job job, string orignialImportApiSettings)
		{
			var importSettings = JsonConvert.DeserializeObject<ImportSettings>(orignialImportApiSettings);
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

		private List<IBatchStatus> InitializeExportServiceJobObservers(Job job)
		{
			string tempTableName = $"{job.JobId}_{_identifier}";
			ITempDocTableHelper docTableHelper = _tempDocumentTableFactory.GetDocTableHelper(tempTableName, _sourceConfiguration.SourceWorkspaceArtifactId);

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(docTableHelper,
				_sourceWorkspaceManager, _sourceJobManager,
				_documentRepository, _synchronizerFactory,
				MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration,
				IntegrationPointDto.DestinationConfiguration, JobHistoryDto.ArtifactId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTaggerDestinationWorkspace = new DestinationWorkspaceManager(_tempDocumentTableFactory, _repositoryFactory, _sourceConfiguration, tempTableName, JobHistoryDto.ArtifactId);
			IConsumeScratchTableBatchStatus sourceJobHistoryTagger = new JobHistoryManager(_tempDocumentTableFactory, _repositoryFactory, JobHistoryDto.ArtifactId, _sourceConfiguration.SourceWorkspaceArtifactId, tempTableName);

			var batchStatusCommands = new List<IBatchStatus>()
			{
				destinationFieldsTagger,
				sourceFieldsTaggerDestinationWorkspace,
				sourceJobHistoryTagger
			};
			return batchStatusCommands;
		}
	}
}