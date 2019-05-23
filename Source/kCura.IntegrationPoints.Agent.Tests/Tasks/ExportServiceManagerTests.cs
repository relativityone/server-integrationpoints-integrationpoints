﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	[NUnit.Framework.Description("IMPORTANT" +
				 "These existing tests will show that they cover majority of the code. " +
				 "But the tests below are only consist of the stopping scenarios and regular gold flow." +
				 "A lot more tests must be added !")]
	public class ExportServiceManagerTests : TestBase
	{
		private const int _RETRY_SAVEDSEARCHID = 312;
		private const int _EXPORT_DOC_COUNT = 0;
		private const string _IMPORTSETTINGS_WITH_USERID = "blah blah blah";

		private ExportServiceManager _instance;
		private IHelper _helper;
		private IHelperFactory _helperFactory;
		private ICaseServiceContext _caseContext;
		private IContextContainerFactory _contextContainerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private IExporterFactory _exporterFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _claimPrincipleFactory;
		private ITagsCreator _tagsCreator;
		private ITagSavedSearchManager _tagSavedSearchManager;
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
		private ISavedSearchQueryRepository _savedSearchQueryRepository;
		private IJobStopManager _jobStopManager;
		private IDocumentRepository _documentRepository;
		private IWorkspaceRepository _workspaceRepository;
		private IExporterService _exporterService;
		private IIntegrationPointRepository _integrationPointRepository;

		private Job _job;
		private Data.IntegrationPoint _integrationPoint;
		private SourceConfiguration _configuration;
		private TaskParameters _taskParameters;
		private JobHistory _jobHistory;
		private SourceProvider _sourceProvider;
		private List<FieldMap> _mappings;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private JobStatisticsService _jobStatisticsService;
		private object _lock;
		private IBatchStatus _exportServiceObserver;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IBatchStatus _updateJobHistoryStatus;
		private IBatchStatus _sendingEmailNotification;
		private IDataSynchronizer _synchornizer;
		private IJobHistoryManager _historyManager;
		private IAgentValidator _agentValidator;

		[SetUp]
		public override void SetUp()
		{
			Job job = JobExtensions.CreateJob();
			SetUp(job);
		}


		private void SetUp(Job job)
		{
			_job = job;
			_helper = Substitute.For<IHelper>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_caseContext = Substitute.For<ICaseServiceContext>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_exporterFactory = Substitute.For<IExporterFactory>();
			_claimPrincipleFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_tagsCreator = Substitute.For<ITagsCreator>();
			_tagSavedSearchManager = Substitute.For<ITagSavedSearchManager>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

			_sendingEmailNotification = Substitute.For<IBatchStatus>();
			_updateJobHistoryStatus = Substitute.For<IBatchStatus>();
			_batchStatuses = new List<IBatchStatus>() { _sendingEmailNotification, _updateJobHistoryStatus };
			_serializer = Substitute.For<ISerializer>();
			_jobService = Substitute.For<IJobService>();
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_jobHistoryErrorManager = Substitute.For<IJobHistoryErrorManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_savedSearchQueryRepository = Substitute.For<ISavedSearchQueryRepository>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_documentRepository = Substitute.For<IDocumentRepository>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_exporterService = Substitute.For<IExporterService>();
			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_exportServiceObserver = Substitute.For<IBatchStatus>();
			_synchornizer = Substitute.For<IDataSynchronizer>();
			_historyManager = Substitute.For<IJobHistoryManager>();
			_agentValidator = Substitute.For<IAgentValidator>();
			_jobStatisticsService = Substitute.For<JobStatisticsService>();
			var sourceWorkspaceTagsCreator = Substitute.For<ISourceWorkspaceTagCreator>();

			_exporterFactory.InitializeExportServiceJobObservers(Arg.Any<Job>(), _tagsCreator, _tagSavedSearchManager,
				_synchronizerFactory, _serializer, _jobHistoryErrorManager, _jobStopManager, sourceWorkspaceTagsCreator,
				Arg.Any<FieldMap[]>(), Arg.Any<SourceConfiguration>(), Arg.Any<JobHistoryErrorDTO.UpdateStatusType>(),
				Arg.Any<JobHistory>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(new List<IBatchStatus>() { _exportServiceObserver });

			_lock = new object();
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				DestinationConfiguration = "destination config",
				SourceProvider = 741,
				FieldMappings = "mapping",
				SecuredConfiguration = "secured config"
			};
			_configuration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = 8465,
				SavedSearchArtifactId = 987654,
				TypeOfExport = SourceConfiguration.ExportType.SavedSearch
			};

			_taskParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun, TotalItems = 0 };
			_sourceProvider = new SourceProvider();
			_mappings = new List<FieldMap>();
			_updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			_integrationPointRepository.ReadAsync(job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(_integrationPoint.SourceConfiguration).Returns(_configuration);
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(_taskParameters);
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(_jobHistory);
			_caseContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_mappings);
			_managerFactory.CreateJobHistoryErrorManager(_contextContainer, _configuration.SourceWorkspaceArtifactId, GetUniqueJobId(job, _taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
			_jobHistoryErrorManager.StageForUpdatingErrors(job, Arg.Is<Choice>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(_updateStatusType);
			_repositoryFactory.GetSavedSearchQueryRepository(_configuration.SourceWorkspaceArtifactId).Returns(_savedSearchQueryRepository);
			_savedSearchQueryRepository.RetrieveSavedSearch(_configuration.SavedSearchArtifactId).Returns(new SavedSearchDTO());
			_repositoryFactory.GetJobHistoryErrorRepository(_configuration.SourceWorkspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, _configuration.SavedSearchArtifactId).Returns(_RETRY_SAVEDSEARCHID);
			_synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _integrationPoint.DestinationConfiguration, _integrationPoint.SecuredConfiguration).Returns(_synchornizer);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParameters.BatchInstance, job.JobId, true).Returns(_jobStopManager);

			ImportSettings settings = new ImportSettings();
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(settings);
			_serializer.Serialize(settings).Returns(_IMPORTSETTINGS_WITH_USERID);

			_repositoryFactory.GetDocumentRepository(_configuration.SourceWorkspaceArtifactId).Returns(_documentRepository);

			_exporterFactory.BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration,
				_configuration.SavedSearchArtifactId, job.SubmittedBy, _IMPORTSETTINGS_WITH_USERID).Returns(_exporterService);

			_exporterService.TotalRecordsFound.Returns(_EXPORT_DOC_COUNT);
			_jobStopManager.SyncRoot.Returns(_lock);
			_serializer.Deserialize<TaskParameters>(job.JobDetails)
				.Returns(_taskParameters);
			_jobHistoryService.GetRdo(Arg.Is<Guid>(guid => guid == _taskParameters.BatchInstance)).Returns(_jobHistory);

			_managerFactory.CreateTagsCreator(Arg.Any<IContextContainer>()).Returns(_tagsCreator);
			_managerFactory.CreateTaggingSavedSearchManager(Arg.Any<IContextContainer>()).Returns(_tagSavedSearchManager);
			_managerFactory.CreateSourceWorkspaceTagsCreator(Arg.Any<IContextContainer>(), Arg.Any<IHelper>(), Arg.Any<SourceConfiguration>()).Returns(sourceWorkspaceTagsCreator);

			_instance = new ExportServiceManager(_helper, _helperFactory,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory, _exporterFactory,
				_claimPrincipleFactory, _repositoryFactory,
				_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService,
				_jobStatisticsService,
				null,
				_agentValidator,
				_integrationPointRepository);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_historyManager);
		}

		[Test]
		public void Execute_FailToLoadIntegrationPointRDO()
		{
			// ARRANGE
			_integrationPointRepository.ReadAsync(_job.RelatedObjectArtifactID).Returns((Data.IntegrationPoint) null);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), Arg.Is<ArgumentException>(ex => ex.Message == "Failed to retrieve corresponding Integration Point."));
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
			_savedSearchQueryRepository.RetrieveSavedSearch(_configuration.SavedSearchArtifactId).Returns((SavedSearchDTO)null);

			// ACT
			Assert.Throws<IntegrationPointsException>(() => _instance.Execute(_job));

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), Arg.Is<Exception>(ex => ex.Message == Core.Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS));
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
			_exportServiceObserver.When(observer => observer.OnJobStart(_job)).Do(info => { throw new Exception(exceptionMessage); });

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
				_RETRY_SAVEDSEARCHID, _job.SubmittedBy, _IMPORTSETTINGS_WITH_USERID);
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
		[NUnit.Framework.Description("This happens when GeneralWithEntityRdoSynchronizerFactory is passed in.")]
		public void Execute_CreateDestinationProvider_MakeSureToSetSourceProvider()
		{
			// ARRANGE
			IWindsorContainer windsorContainer = Substitute.For<IWindsorContainer>();
			IObjectTypeRepository objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			IRsapiClientFactory rsapiClientFactory = Substitute.For<IRsapiClientFactory>();
			_synchronizerFactory = Substitute.For<GeneralWithEntityRdoSynchronizerFactory>(windsorContainer, objectTypeRepository, rsapiClientFactory);

			// ACT
			ExportServiceManager instance = new ExportServiceManager(_helper, _helperFactory,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory, _exporterFactory,
				_claimPrincipleFactory, _repositoryFactory,
				_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService,
				_jobStatisticsService,
				null,
				_agentValidator,
				_integrationPointRepository);
			try
			{
				instance.Execute(_job);
			}
			catch (Exception)
			{
				// Ignore any errors - we want to check just the assertions below
			}

			// ASSERT
			var factory = _synchronizerFactory as GeneralWithEntityRdoSynchronizerFactory;
			Assert.IsNotNull(factory);
			Assert.AreSame(factory.SourceProvider, _sourceProvider);
		}

		[Test]
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_FailToSetJobStateAsUnstoppable_OnFinalizeExportServiceObservers()
		{
			// ARRANGE
			_jobService.When(service =>
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
			_exporterFactory.DidNotReceive().BuildExporter(Arg.Any<IJobStopManager>(), Arg.Any<FieldMap[]>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), _IMPORTSETTINGS_WITH_USERID);

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
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(type => type.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), exception);
		}

		[Test]
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
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
		[NUnit.Framework.Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void Execute_MakeSureToUpdateJobStopStateToNoneOnScheduledJob()
		{
			// ARRANGE
			_job.SerializedScheduleRule = "rules!";

			// ACT
			_instance.Execute(_job);

			// ASSERT
			EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
			_jobService.Received(1).UpdateStopState(Arg.Is<List<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
		}

		[Test]
		public void Execute_GoldFlow_CreateDataReaderAndPassItToSynchronizer()
		{
			// ARRANGE
			IDataTransferContext reader = Substitute.For<IDataTransferContext>();

			_exporterService.TotalRecordsFound.Returns(99);
			_exporterService.GetDataTransferContext(Arg.Any<IExporterTransferConfiguration>()).Returns(reader);

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_synchornizer.Received(1).SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<List<FieldMap>>(), Arg.Any<string>());
		}

		[Test]
		public void Execute_JobHasNoBatchId_ExpectNewBatchIdToBeGenerated()
		{
			// ARRANGE
			const string newConfig = "new config";
			Job job = new JobBuilder()
				.WithWorkspaceId(_configuration.SourceWorkspaceArtifactId)
				.WithRelatedObjectArtifactId(_integrationPoint.ArtifactId)
				.WithJobDetails(string.Empty)
				.Build();
			SetUp(job);
			_serializer.Serialize(Arg.Any<TaskParameters>()).Returns(newConfig);
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(_jobHistory);

			// ACT
			_instance.Execute(job);

			// ASSERT
			Assert.AreEqual(newConfig, _job.JobDetails);
		}

		[Test]
		public void Execute_EnsureToValidateJob()
		{
			// ARRANGE


			// ACT
			_instance.Execute(_job);

			// ASSERT
			_agentValidator.Received(1).Validate(_integrationPoint, _job.SubmittedBy);


			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(x => x == _jobHistory));
		}

		[Test]
		public void Execute_EnsureToHandleValidationErrorJob()
		{
			// ARRANGE
			_agentValidator.When(x => x.Validate(_integrationPoint, _job.SubmittedBy)).Do(x =>
				{
					throw new PermissionException();
				}
			);

			bool jobValidationFailedUpdated = false;
			_jobHistoryService.When(x => x.UpdateRdo(Arg.Is<JobHistory>(jh => jh.JobStatus.Guids.First() == JobStatusChoices.JobHistoryValidationFailed.Guids.First()))).Do(item =>
				{
					jobValidationFailedUpdated = true;
				}

			);
			// ACT
			Assert.Throws<PermissionException>(() => _instance.Execute(_job));

			// ASSERT
			_agentValidator.Received(1).Validate(_integrationPoint, _job.SubmittedBy);

			// job status should be changed
			Assert.That(jobValidationFailedUpdated);

			// we expect to first change state to Validating and then Validation Failed
			_jobHistoryService.Received(2).UpdateRdo(Arg.Is<JobHistory>(x => x == _jobHistory));
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
				_exporterFactory.Received(1).BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration, _RETRY_SAVEDSEARCHID, _job.SubmittedBy, _IMPORTSETTINGS_WITH_USERID);
				_jobHistoryErrorManager.Received(1).CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
				_jobHistoryErrorRepository.Received(1).DeleteItemLevelErrorsSavedSearch(_RETRY_SAVEDSEARCHID);
			}
			else
			{
				_exporterFactory.Received(1).BuildExporter(_jobStopManager, Arg.Any<FieldMap[]>(), _integrationPoint.SourceConfiguration, _configuration.SavedSearchArtifactId, _job.SubmittedBy, _IMPORTSETTINGS_WITH_USERID);
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