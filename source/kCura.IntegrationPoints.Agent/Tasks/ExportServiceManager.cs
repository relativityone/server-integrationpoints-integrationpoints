﻿using System;
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
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ITask
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly JobHistoryService _jobHistoryService;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private ExportJobErrorService _exportJobErrorService;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly IExporterFactory _exporterFactory;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ITempDocumentTableFactory _tempDocumentTableFactory;
		private readonly IDocumentRepository _documentRepository;
		private readonly JobStatisticsService _statisticsService;
		private readonly List<IBatchStatus> _batchStatus;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly IJobService _jobService;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;
		private Guid _identifier;
		private SourceConfiguration _sourceConfiguration;
		private ITempDocTableHelper _docTableHelper;
		private List<IBatchStatus> _parallelizableBatch;
		private IConsumeScratchTableBatchStatus _destinationFieldsTagger;
		private IConsumeScratchTableBatchStatus _sourceFieldsTaggerDestinationWorkspace;
		private JobHistoryManager _sourceJobHistoryTagger;
		private readonly TaskResult _taskResult;

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
			_synchronizerFactory = synchronizerFactory;
			_exporterFactory = exporterFactory;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_sourceJobManager = sourceJobManager;
			_repositoryFactory = repositoryFactory;
			_tempDocumentTableFactory = tempDocumentTableFactory;
			_documentRepository = documentRepository;
			_caseServiceContext = caseServiceContext;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_statisticsService = statisticsService;
			_batchStatus = statuses.ToList();
			_serializer = serializer;
			_jobService = jobService;
			_scheduleRuleFactory = scheduleRuleFactory;
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

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				IDataSynchronizer synchronizer = GetRdoDestinationProvider(destinationConfig);

				IScratchTableRepository[] scratchTableRepositories = new[]
				{
					_sourceFieldsTaggerDestinationWorkspace.ScratchTableRepository
				};

				_exportJobErrorService = new ExportJobErrorService(scratchTableRepositories);
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
						IScratchTableRepository[] scratchTables = new[]
						{
							_destinationFieldsTagger.ScratchTableRepository,
							_sourceJobHistoryTagger.ScratchTableRepository,
							_sourceFieldsTaggerDestinationWorkspace.ScratchTableRepository
						};

						IDataReader dataReader = exporter.GetDataReader(scratchTables);
						synchronizer.SyncData(dataReader, MappedFields, destinationConfig);
					}
				}

				TagDocuments(job);
			}
			catch (Exception ex)
			{
				_taskResult.Status = TaskStatusEnum.Fail;
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		protected void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
			_exportJobErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void TagDocuments(Job job)
		{
			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_parallelizableBatch, batch =>
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

			Exception ex = ConstructNewExceptionIfAny(exceptions);
			if (ex != null)
			{
				throw ex;
			}
		}

		private void InitializeExportService(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			this._identifier = taskParameters.BatchInstance;

			// Load integrationPoint data
			if (IntegrationPointDto != null)
			{
				return;
			}

			int integrationPointId = job.RelatedObjectArtifactID;
			this.IntegrationPointDto = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);

			if (this.IntegrationPointDto == null)
			{
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}

			_sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);

			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			string tempTableName = $"{job.JobId}_{_identifier}";
			_docTableHelper = _tempDocumentTableFactory.GetDocTableHelper(tempTableName, _sourceConfiguration.SourceWorkspaceArtifactId);

			this.JobHistoryDto = _jobHistoryService.CreateRdo(this.IntegrationPointDto, this._identifier, DateTime.UtcNow);
			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;
			UpdateJobStatus();

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_docTableHelper,
				_sourceWorkspaceManager, _sourceJobManager,
				_documentRepository, _synchronizerFactory, MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration, IntegrationPointDto.DestinationConfiguration, JobHistoryDto.ArtifactId);

			_destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			_sourceFieldsTaggerDestinationWorkspace = new DestinationWorkspaceManager(_tempDocumentTableFactory, _repositoryFactory, _sourceConfiguration, tempTableName, JobHistoryDto.ArtifactId);
			_sourceJobHistoryTagger = new JobHistoryManager(_tempDocumentTableFactory, _repositoryFactory, JobHistoryDto.ArtifactId, _sourceConfiguration.SourceWorkspaceArtifactId, tempTableName);

			_parallelizableBatch = new List<IBatchStatus>()
			{
				_destinationFieldsTagger,
				_sourceFieldsTaggerDestinationWorkspace,
				_sourceJobHistoryTagger
			};

			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_parallelizableBatch, batch =>
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

			Exception ex = ConstructNewExceptionIfAny(exceptions);
			if (ex != null)
			{
				throw ex;
			}

			_batchStatus.ForEach(batch => batch.JobStarted(job));
		}

		internal void PostExecute(Job job)
		{
			try
			{
				_destinationFieldsTagger.ScratchTableRepository.Dispose();
				_sourceJobHistoryTagger.ScratchTableRepository.Dispose();
				_sourceFieldsTaggerDestinationWorkspace.ScratchTableRepository.Dispose();
			}
			catch(Exception) {
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
					this.IntegrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory,
						_taskResult);
				}
				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
			}
			catch
			{
				// ignored
			}
		}

		internal IDataSynchronizer GetRdoDestinationProvider(string configuration)
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

		private Exception ConstructNewExceptionIfAny(IEnumerable<Exception> exceptions)
		{
			if (exceptions == null)
			{
				return null;
			}
			
			Exception ex = null;
			Exception[] enumerable = exceptions as Exception[] ?? exceptions.ToArray();
			if (!enumerable.IsNullOrEmpty())
			{
				int counter = 0;
				string message = String.Join(Environment.NewLine, enumerable.Select(exception => $"{++counter}. {exception.Message}"));
				ex = new AggregateException(message, enumerable);
			}

			return ex;
		}
	}
}