using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

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
		private readonly ITargetWorkspaceJobHistoryManager _targetWorkspaceJobHistoryManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ITempDocumentTableFactory _tempDocumentTableFactory;
		private readonly IDocumentRepository _documentRepository;
		private readonly JobStatisticsService _statisticsService;
		private readonly List<IBatchStatus> _batchStatus;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private Guid _identifier;
		private SourceConfiguration _sourceConfiguration;
		private ITempDocTableHelper _docTableHelper;
		private List<IConsumeScratchTableBatchStatus> _parallizableBatch;
		private IConsumeScratchTableBatchStatus _destinationFieldsTagger;
		private IConsumeScratchTableBatchStatus _sourceDestinationWorkspaceTagger;
		private JobHistoryManager _sourceJobHistoryTagger;

		public ExportServiceManager(
			ICaseServiceContext caseServiceContext,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ITargetWorkspaceJobHistoryManager targetWorkspaceJobHistoryManager,
			ITempDocumentTableFactory tempDocumentTableFactory,
			IRepositoryFactory repositoryFactory,
			IEnumerable<IBatchStatus> statuses,
			IDocumentRepository documentRepository,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			JobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService)
		{
			_synchronizerFactory = synchronizerFactory;
			_exporterFactory = exporterFactory;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_targetWorkspaceJobHistoryManager = targetWorkspaceJobHistoryManager;
			_repositoryFactory = repositoryFactory;
			_tempDocumentTableFactory = tempDocumentTableFactory;
			_documentRepository = documentRepository;
			_caseServiceContext = caseServiceContext;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_statisticsService = statisticsService;
			_batchStatus = statuses.ToList();
			_serializer = serializer;
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
					_destinationFieldsTagger.ScratchTableRepository,
					_sourceDestinationWorkspaceTagger.ScratchTableRepository
				};
				_exportJobErrorService = new ExportJobErrorService(scratchTableRepositories);
				SetupSubscriptions(synchronizer, job);

				// Initialize Exporter
				IExporterService exporter = _exporterFactory.BuildExporter(MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration);
				JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
				UpdateJobStatus();

				if (exporter.TotalRecordsFound > 0)
				{

					IScratchTableRepository[] scratchTables = new[]
					{
						_destinationFieldsTagger.ScratchTableRepository,
						_sourceJobHistoryTagger.ScratchTableRepository,
						_sourceDestinationWorkspaceTagger.ScratchTableRepository
					};
					IDataReader dataReader = exporter.GetDataReader(scratchTables);
					synchronizer.SyncData(dataReader, MappedFields, destinationConfig);
				}
			}
			catch (Exception ex)
			{
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

			_docTableHelper = _tempDocumentTableFactory.GetDocTableHelper(this._identifier.ToString(), _sourceConfiguration.SourceWorkspaceArtifactId);

			this.JobHistoryDto = _jobHistoryService.GetRdo(this._identifier);
			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;
			UpdateJobStatus();

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_docTableHelper,
				_sourceWorkspaceManager, _targetWorkspaceJobHistoryManager,
				_documentRepository, _synchronizerFactory, MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration, IntegrationPointDto.DestinationConfiguration, JobHistoryDto.ArtifactId);

			_destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			_sourceDestinationWorkspaceTagger = new DestinationWorkspaceManager(_tempDocumentTableFactory, _repositoryFactory, _sourceConfiguration, _identifier.ToString(), JobHistoryDto.ArtifactId);
			_sourceJobHistoryTagger = new JobHistoryManager(_tempDocumentTableFactory, _repositoryFactory, JobHistoryDto.ArtifactId, _sourceConfiguration.SourceWorkspaceArtifactId, _identifier.ToString());

			_parallizableBatch = new List<IConsumeScratchTableBatchStatus>()
			{
				_destinationFieldsTagger,
				_sourceDestinationWorkspaceTagger,
				_sourceJobHistoryTagger
			};

			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_parallizableBatch, batch =>
			{
				try
				{
					batch.JobStarted(job);
				}
				catch (Exception exception)
				{
					exceptions.Enqueue(exception);
					throw;
				}
			});

			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}

			_batchStatus.ForEach(batch => batch.JobStarted(job));
		}

		internal void PostExecute(Job job)
		{
			var exceptions = new ConcurrentQueue<Exception>();
			Parallel.ForEach(_parallizableBatch, batch =>
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

			if (exceptions.Count > 0)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, new AggregateException(exceptions));
				_jobHistoryErrorService.CommitErrors();
			}

			foreach (IBatchStatus completedItem in _batchStatus)
			{
				try
				{
					completedItem.JobComplete(job);
				}
				catch (Exception e)
				{
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
			Guid providerGuid = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
			GeneralWithCustodianRdoSynchronizerFactory factory = _synchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(providerGuid, configuration);
			return synchronizer;
		}

		private void UpdateJobStatus()
		{
			_jobHistoryService.UpdateRdo(JobHistoryDto);
		}
	}
}