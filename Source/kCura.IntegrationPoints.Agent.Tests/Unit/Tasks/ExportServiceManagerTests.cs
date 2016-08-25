﻿using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
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
using System.Data;
using System.Linq;
using System;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	[Description("IMPORTANT" +
	             "These existing tests will show that they cover majority of the code. " +
	             "But the tests below are only consist of the stopping scenarios and regular gold flow." +
	             "A lot more tests must be added !")]
	public class ExportServiceManagerTests
	{
		private const int _RETRY_SAVEDSEARCHID = 312;
		private const int _EXPORT_DOC_COUNT = 0;
		private const string _IMPORTSETTINGS_WITH_USERID = "blah blah blah";

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
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IBatchStatus _updateJobHistoryStatus;
		private IBatchStatus _sendingEmailNotification;
		private IDataSynchronizer _synchornizer;
		private IJobHistoryManager _historyManager;

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

			_sendingEmailNotification = Substitute.For<IBatchStatus>();
			_updateJobHistoryStatus = Substitute.For<IBatchStatus>();
			_batchStatuses = new List<IBatchStatus>() {_sendingEmailNotification, _updateJobHistoryStatus};
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
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_exportServiceObserver = Substitute.For<IBatchStatus>();
			_synchornizer = Substitute.For<IDataSynchronizer>();
			_historyManager = Substitute.For<IJobHistoryManager>();

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
				FieldMappings = "mapping",
			};
			_configuration = new SourceConfiguration()
			{
				SavedSearchArtifactId = 987654
			};

			_taskParameters = new TaskParameters();
			_jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun, TotalItems = 0};
			_sourceProvider = new SourceProvider();
			_mappings = new List<FieldMap>();
			_updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			_caseContext.RsapiService.IntegrationPointLibrary.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(_integrationPoint.SourceConfiguration).Returns(_configuration);
			_serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(_taskParameters);
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(_jobHistory);
			_caseContext.RsapiService.SourceProviderLibrary.Read(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_mappings);
			_managerFactory.CreateJobHistoryErrorManager(_contextContainer, _configuration.SourceWorkspaceArtifactId, GetUniqueJobId(_job, _taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
			_jobHistoryErrorManager.StageForUpdatingErrors(_job, Arg.Is<Choice>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(_updateStatusType);
			_repositoryFactory.GetSavedSearchRepository(_configuration.SourceWorkspaceArtifactId, _configuration.SavedSearchArtifactId).Returns(_savedSearchRepository);
			_savedSearchRepository.RetrieveSavedSearch().Returns(new SavedSearchDTO());
			_repositoryFactory.GetJobHistoryErrorRepository(_configuration.SourceWorkspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId).Returns(_RETRY_SAVEDSEARCHID);
			_synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _integrationPoint.DestinationConfiguration).Returns(_synchornizer);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParameters.BatchInstance, _job.JobId, true).Returns(_jobStopManager);

			ImportSettings settings = new ImportSettings();
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(settings);
			_serializer.Serialize(settings).Returns(_IMPORTSETTINGS_WITH_USERID);

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
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_historyManager);

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
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_StopAtTheVeryBeginningOfTheJob()
		{
			// ARRANGE
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(info => { throw new OperationCanceledException(); });

			// ACT
			_instance.Execute(_job);

			// ASSERT
			Assert.AreEqual(0, _jobHistory.TotalItems);
			AssertFinalizedJob(_job);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_StopAfterAcquiringTheSynchronizer()
		{
			// ARRANGE
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(Callback.First(x => { })
				.Then(info => { throw new OperationCanceledException(); }));

			// ACT
			_instance.Execute(_job);

			// ASSERT
			Assert.AreEqual(0, _jobHistory.TotalItems);
			AssertFinalizedJob(_job);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
			Assert.AreEqual(0, _jobHistory.TotalItems);
			_jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<Choice>(), Arg.Any<Exception>());
			_jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<Choice>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
			AssertFinalizedJob(_job);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_NoStopRequested()
		{
			// ACT
			_instance.Execute(_job);

			// ASSERT
			EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
			EnsureToUpdateTotalItemCount();
			ExporterServiceObjectIsFinalized();
			// do not tag any errors
			_historyManager.DidNotReceive().SetErrorStatusesToExpired(Arg.Any<int>(), Arg.Any<int>());
			AssertFinalizedJob(_job);
		}

		[Test]
		public void Execute_ExportServiceJobObserverFailToInitialize()
		{
			// ARRANGE
			const string exceptionMessage = "exception !";
			_exportServiceObserver.When( observer => observer.OnJobStart(_job)).Do(info => { throw new Exception(exceptionMessage); });

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), Arg.Is<AggregateException>(ex => ex.InnerExceptions[0].Message == exceptionMessage));
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();

		}

		[Test]
		public void Execute_ExportServiceJobObserverFailToFinalized()
		{
			// ARRANGE
			const string exceptionMessage = "exception !";
			_exportServiceObserver.When(observer => observer.OnJobComplete(_job)).Do(info => { throw new Exception(exceptionMessage); });

			// ACT
			_instance.Execute(_job);

			// ASSERT
			AssertFinalizedJob(_job);
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), Arg.Is<AggregateException>(ex => ex.InnerExceptions[0].Message == exceptionMessage));
		}

		[Test]
		public void Execute_NewSavedSearchIsCreatedOnItemLevelErrorRetry()
		{
			// ARRANGE
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_exporterFactory.Received(1).BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(),
				_integrationPoint.SourceConfiguration,
				_RETRY_SAVEDSEARCHID, _job.SubmittedBy);
			AssertRetrySavedSearch(true);
		}

		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
		[TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
		public void Execute_NoRetrySavedSearchCreated(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices errorType, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices jobType)
		{
			// ARRANGE
			_updateStatusType.ErrorTypes = errorType;
			_updateStatusType.JobType = jobType;

			// ACT
			_instance.Execute(_job);

			// ASSERT
			AssertRetrySavedSearch(false);
		}

		[Test]
		[Description("This happens when GeneralWithCustodianRdoSynchronizerFactory is passed in.")]
		public void Execute_CreateDestinationProvider_MakeSureToSetSourceProvider()
		{
			// ARRANGE
			IWindsorContainer windsorContainer = Substitute.For<IWindsorContainer>();
			IRSAPIClient rsapiClient = Substitute.For<IRSAPIClient>();
			RSAPIRdoQuery rdoQuery = new RSAPIRdoQuery(rsapiClient);
			_synchronizerFactory = Substitute.For<GeneralWithCustodianRdoSynchronizerFactory>(windsorContainer, rdoQuery);

			// ACT
			ExportServiceManager instance = new ExportServiceManager(_helper,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory, _exporterFactory,
				_claimPrincipleFactory, _sourceWorkspaceManager,
				_sourceJobManager, _repositoryFactory,
				_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService,
				null);
			instance.Execute(_job);

			// ASSERT
			var factory = _synchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			Assert.IsNotNull(factory);
			Assert.AreSame(factory.SourceProvider, _sourceProvider); 
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_FailToSetJobStateAsUnstoppable_OnFinalizeExportServiceObservers()
		{
			// ARRANGE
			_jobService.When( service =>
				service.UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable))
				.Throw<Exception>();

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_exportServiceObserver.Received(1).OnJobComplete(_job);
		}

		[Test]
		public void Execute_GoldFlow_DefaultBatchStatus()
		{
			// ACT
			_instance.Execute(_job);

			// ASSERT
			_sendingEmailNotification.Received(1).OnJobStart(_job);
			_sendingEmailNotification.Received(1).OnJobComplete(_job);

			_updateJobHistoryStatus.Received(1).OnJobStart(_job);
			_updateJobHistoryStatus.Received(1).OnJobComplete(_job);

		}

		[Test]
		public void Execute_DefaultBatchStatus_ErrorOnStart()
		{
			// ARRANGE
			_sendingEmailNotification.When(notifer => notifer.OnJobStart(_job)).Throw<Exception>();

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_exporterFactory.DidNotReceive().BuildExporter(Arg.Any<IJobStopManager>(), Arg.Any<FieldMap[]>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());

			_sendingEmailNotification.Received(1).OnJobComplete(_job);
			_updateJobHistoryStatus.Received(1).OnJobStart(_job);
			_updateJobHistoryStatus.Received(1).OnJobComplete(_job);
		}

		[Test]
		public void Execute_DefaultBatchStatus_ErrorOnComplete()
		{
			// ARRANGE
			Exception exception = new Exception();
			_sendingEmailNotification.When(notifer => notifer.OnJobComplete(_job)).Do(info => { throw exception; });

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_updateJobHistoryStatus.Received(1).OnJobComplete(_job);
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>( type => type.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), exception);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_EnsureToMarkErrorStatusAsExpiredIfTheJobIsStopped()
		{
			// ARRAGE
			_jobStopManager.IsStopRequested().Returns(true);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
			_historyManager.Received(1).SetErrorStatusesToExpired(_caseContext.WorkspaceID, _jobHistory.ArtifactId);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_FailMarkErrorStatusAsExpiredIfTheJobIsStopped_ExpectNoException()
		{
			// ARRAGE
			_jobStopManager.IsStopRequested().Returns(true);
			_historyManager.When(manager => manager.SetErrorStatusesToExpired(_caseContext.WorkspaceID, _jobHistory.ArtifactId))
				.Throw<Exception>();

			// ACT &  ASSERT
			_instance.Execute(_job);
			EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();

		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_MakeSureToUpdateJobStopStateToNoneOnScheduledJob()
		{
			// ARRANGE
			_job.SerializedScheduleRule = "rules!";

			// ACT
			_instance.Execute(_job);

			// ASSERT
			EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
			_jobService.Received(1).UpdateStopState(Arg.Is<List<long>>(lst => lst.SequenceEqual(new [] { _job.JobId })), StopState.None);
		}

		[Test]
		public void Execute_GoldFlow_CreateDataReaderAndPassItToSynchronizer()
		{
			// ARRANGE
			IDataReader reader = Substitute.For<IDataReader>();

			_exporterService.TotalRecordsFound.Returns(99);
			_exporterService.GetDataReader(Arg.Any<IScratchTableRepository[]>()).Returns(reader);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_synchornizer.Received(1).SyncData(Arg.Any<IDataReader>(), Arg.Any<List<FieldMap>>(), Arg.Any<string>());
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

		private void AssertRetrySavedSearch(bool expectToCreate)
		{
			if (expectToCreate)
			{
				_exporterFactory.Received(1).BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration, _RETRY_SAVEDSEARCHID, _job.SubmittedBy);
				_jobHistoryErrorManager.Received(1).CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
				_jobHistoryErrorRepository.Received(1).DeleteItemLevelErrorsSavedSearch(_RETRY_SAVEDSEARCHID);
			}
			else
			{
				_exporterFactory.Received(1).BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration, _configuration.SavedSearchArtifactId, _job.SubmittedBy);
				_jobHistoryErrorManager.DidNotReceive().CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
				_jobHistoryErrorRepository.DidNotReceive().DeleteItemLevelErrorsSavedSearch(_RETRY_SAVEDSEARCHID);
			}
		}

		private void EnsureToPassTheJobStopManagerToTheJobHistroyErrorService()
		{
			_jobHistoryErrorService.Received(1).JobStopManager = _jobStopManager;
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