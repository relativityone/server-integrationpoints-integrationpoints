using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using System.Collections.Generic;
using System;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class ExportServiceManagerTests
	{
		private const int _RETRY_SAVEDSEARCHID = 312;
		private const int _EXPORT_DOC_COUNT = 0;

		private ExportServiceManager _instance;
		private IHelper _helper;
		private ICaseServiceContext _caseContext;
		private IContextContainerFactory _contextContainerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private IExporterFactory _exporterFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _claimPrincipleFactory;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IRepositoryFactory _repositoryFactory;
		private IManagerFactory _managerFactory;
		private IEnumerable<IBatchStatus> _batchStatuses;
		private ISerializer _serializer;
		private IJobService _jobService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IJobHistoryManager _jobHistoryManager;
		private IContextContainer _contextContainer;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private ISavedSearchRepository _savedSearchRepository;
		private IJobStopManager _jobStopManager;
		private IDocumentRepository _documentRepository;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private IWorkspaceRepository _workspaceRepository;
		private IExporterService _exporterService;

		private Job _job;
		private Data.IntegrationPoint _integrationPoint;
		private SourceConfiguration _configuration;
		private TaskParameters _taskParameters;
		private JobHistory _jobHistory;
		private SourceProvider _sourceProvider;
		private List<FieldMap> _mappings;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private object _lock;
		private IBatchStatus _exportServiceObserver;

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_caseContext = Substitute.For<ICaseServiceContext>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_exporterFactory = Substitute.For<IExporterFactory>();
			_claimPrincipleFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_batchStatuses = Substitute.For<IEnumerable<IBatchStatus>>();
			_serializer = Substitute.For<ISerializer>();
			_jobService = Substitute.For<IJobService>();
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_jobHistoryErrorManager = Substitute.For<IJobHistoryErrorManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_documentRepository = Substitute.For<IDocumentRepository>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_exporterService = Substitute.For<IExporterService>();
			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);

			_exportServiceObserver = Substitute.For<IBatchStatus>();

			_exporterFactory.InitializeExportServiceJobObservers(Arg.Any<Job>(), _sourceWorkspaceManager, _sourceJobManager,
				_synchronizerFactory, _serializer, _jobHistoryErrorManager,
				Arg.Any<FieldMap[]>(), Arg.Any<SourceConfiguration>(), Arg.Any<JobHistoryErrorDTO.UpdateStatusType>(),
				Arg.Any<Data.IntegrationPoint>(), Arg.Any<JobHistory>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(new List<IBatchStatus>() { _exportServiceObserver });

			_lock = new object();
			_job = JobExtensions.CreateJob();
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				DestinationConfiguration = "destination config",
				SourceProvider = 741,
				FieldMappings = "mapping"
			};
			_configuration = new SourceConfiguration();
			_taskParameters = new TaskParameters();
			_jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun };
			_sourceProvider = new SourceProvider();
			_mappings = new List<FieldMap>();
			_updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			_caseContext.RsapiService.IntegrationPointLibrary.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(_integrationPoint.SourceConfiguration).Returns(_configuration);
			_serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(_taskParameters);
			_jobHistoryService.CreateRdo(_integrationPoint, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(_jobHistory);
			_caseContext.RsapiService.SourceProviderLibrary.Read(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_mappings);
			_managerFactory.CreateJobHistoryErrorManager(_contextContainer, _configuration.SourceWorkspaceArtifactId, GetUniqueJobId(_job, _taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
			_jobHistoryErrorManager.StageForUpdatingErrors(_job, Arg.Is<Choice>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(_updateStatusType);
			_repositoryFactory.GetSavedSearchRepository(_configuration.SourceWorkspaceArtifactId, _configuration.SavedSearchArtifactId).Returns(_savedSearchRepository);
			_savedSearchRepository.RetrieveSavedSearch().Returns(new SavedSearchDTO());
			_jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId).Returns(_RETRY_SAVEDSEARCHID);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParameters.BatchInstance, _job.JobId).Returns(_jobStopManager);
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(new ImportSettings());
			_repositoryFactory.GetDocumentRepository(_configuration.SourceWorkspaceArtifactId).Returns(_documentRepository);

			_exporterFactory.BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration,
				_configuration.SavedSearchArtifactId, _job.SubmittedBy).Returns(_exporterService);

			_exporterService.TotalRecordsFound.Returns(_EXPORT_DOC_COUNT);
			_jobStopManager.SyncRoot.Returns(_lock);
			_instance = new ExportServiceManager(_helper,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory, _exporterFactory,
				_claimPrincipleFactory, _sourceWorkspaceManager,
				_sourceJobManager, _repositoryFactory,
				_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService,
				null);
		}

		[Test]
		public void Execute_FailToLoadIntegrationPointRDO()
		{
			// ARRANGE
			_caseContext.RsapiService.IntegrationPointLibrary.Read(_job.RelatedObjectArtifactID).Returns((Data.IntegrationPoint)null);
			
			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)) , Arg.Is< ArgumentException>(ex => ex.Message == "Failed to retrieved corresponding Integration Point."));
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		public void Execute_EnsureToSanatizeFieldMappings()
		{
			// ARRANGE
			List<FieldMap> mappedFields = new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry() { DisplayName = "source" },
					DestinationField =  new FieldEntry() { DisplayName = "destination"},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(mappedFields);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			Assert.IsTrue(mappedFields[0].SourceField.IsIdentifier);
		}

		[Test]
		public void Execute_EnsureToCheckTheExistentOfSavedSearch()
		{
			// ARRANGE
			_savedSearchRepository.RetrieveSavedSearch().Returns((SavedSearchDTO)null);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), Arg.Is<Exception>(ex => ex.Message == Core.Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS));
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		public void Execute_StopAtTheVeryBeginningOfTheJob()
		{
			// ARRANGE
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(info => { throw new OperationCanceledException(); });

			// ACT
			_instance.Execute(_job);

			// ASSERT
			AssertFinalizedJob(_job);
		}

		[Test]
		public void Execute_StopAfterAcquiringTheSynchronizer()
		{
			// ARRANGE
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(Callback.First(x => { })
				.Then(info => { throw new OperationCanceledException(); }));

			// ACT
			_instance.Execute(_job);

			// ASSERT
			AssertFinalizedJob(_job);
		}

		[Test]
		public void Execute_StopBeforeExecutePushingData()
		{
			// ARRANGE

			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(Callback.First(x => { })
				.Then(x => { })
				.Then(info => { throw new OperationCanceledException(); }));
			
			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<Choice>(), Arg.Any<Exception>());
			_jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<Choice>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

			AssertFinalizedJob(_job);
		}

		[Test]
		public void Execute_NoStopRequested()
		{
			// ACT
			_instance.Execute(_job);

			// ASSERT
			EnsureToUpdateTotalItemCount();
			ExporterServiceObjectIsFinalized();
			AssertFinalizedJob(_job);
		}

		[Test]
		public void Execute_ExportServiceJobObserverFailToInitialize()
		{
			//_exportServiceObserver.When( observer => observer.OnJobStart(_job)).Do(info => throw new Exception(););	
		}

		private void AssertFinalizedJob(Job job)
		{
			// dispose jobStopManager
			_jobStopManager.Received().Dispose();

			// update stop state to unstoppable
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(job.JobId)), StopState.Unstoppable);

			// commit error
			_jobHistoryErrorService.Received().CommitErrors();
		}

		private void EnsureToUpdateTotalItemCount()
		{
			_jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(rdo => rdo.TotalItems == _EXPORT_DOC_COUNT));
		}

		private void ExporterServiceObjectIsFinalized()
		{
			_exportServiceObserver.OnJobComplete(_job);
		}

		private string GetUniqueJobId(Job job, Guid identifier)
		{
			return job.JobId + "_" + identifier;
		}
	}
}