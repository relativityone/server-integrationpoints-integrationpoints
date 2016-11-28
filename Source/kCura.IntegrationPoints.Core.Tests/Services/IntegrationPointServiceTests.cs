using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
	[TestFixture]
	public class IntegrationPointServiceTests : TestBase
	{
		private readonly int _sourceWorkspaceArtifactId = 789;
		private readonly int _targetWorkspaceArtifactId = 9954;
		private readonly int _integrationPointArtifactId = 741;
		private readonly int _savedSearchArtifactId = 93032;
		private readonly int _sourceProviderId = 321;
		private readonly int _destinationProviderId = 424;
		private readonly int _userId = 951;
		private readonly int _previousJobHistoryArtifactId = Int32.MaxValue;

		private IHelper _helper;
		private ICaseServiceContext _caseServiceManager;
		private IContextContainer _contextContainer;
		private IRepositoryFactory _repositoryFactory;
		private IPermissionRepository _sourcePermissionRepository;
		private IPermissionRepository _targetPermissionRepository;
		private IContextContainerFactory _contextContainerFactory;
		private IJobManager _jobManager;
		private IQueueManager _queueManager;
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IManagerFactory _managerFactory;
		private Data.IntegrationPoint _integrationPoint;
		private IntegrationPointDTO _integrationPointDto;
		private SourceProvider _sourceProvider;
		private DestinationProvider _destinationProvider;
		private IIntegrationPointManager _integrationPointManager;
		private IErrorManager _errorManager;
		private IJobHistoryManager _jobHistoryManager;
		private IntegrationPointService _instance;
		private IChoiceQuery _choiceQuery;
		private PermissionCheckDTO _stopPermissionChecksResults;
		private Data.JobHistory _previousJobHistory;
		private IIntegrationModelValidator _integrationModelValidator;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_caseServiceManager = Substitute.For<ICaseServiceContext>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();
			_targetPermissionRepository = Substitute.For<IPermissionRepository>();
			_contextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_serializer = Substitute.For<ISerializer>();
			_jobManager = Substitute.For<IJobManager>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_queueManager = Substitute.For<IQueueManager>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_errorManager = Substitute.For<IErrorManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_integrationModelValidator = Substitute.For<IIntegrationModelValidator>();
			_integrationModelValidator.Validate(Arg.Any<IntegrationModel>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>()).Returns(new ValidationResult());

			_instance = Substitute.ForPartsOf<IntegrationPointService>(
				_helper, 
				_caseServiceManager,
				_contextContainerFactory, 
				_serializer, 
				_choiceQuery, 
				_jobManager,
				_jobHistoryService, 
				_managerFactory,
				_integrationModelValidator
			);

			_caseServiceManager.RsapiService = Substitute.For<IRSAPIService>();
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Returns(Substitute.For<IGenericLibrary<Data.IntegrationPoint>>());
			_caseServiceManager.RsapiService.SourceProviderLibrary.Returns(Substitute.For<IGenericLibrary<SourceProvider>>());
			_caseServiceManager.WorkspaceID = _sourceWorkspaceArtifactId;

			_repositoryFactory.GetPermissionRepository(_sourceWorkspaceArtifactId).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(_targetWorkspaceArtifactId).Returns(_targetPermissionRepository);
			_managerFactory.CreateIntegrationPointManager(Arg.Is(_contextContainer)).Returns(_integrationPointManager);
			_managerFactory.CreateErrorManager(Arg.Is(_contextContainer)).Returns(_errorManager);
			_managerFactory.CreateJobHistoryManager(Arg.Is(_contextContainer)).Returns(_jobHistoryManager);

			_integrationPoint = new Data.IntegrationPoint { ArtifactId = _integrationPointArtifactId, EnableScheduler = false };

			_integrationPoint = new Data.IntegrationPoint
			{
				ArtifactId = _integrationPointArtifactId,
				Name = "IP Name",
				DestinationConfiguration = $"{{ DestinationProviderType : \"{Core.Services.Synchronizer.RdoSynchronizerProvider.RDO_SYNC_TYPE_GUID}\" }}",
				DestinationProvider = _destinationProviderId,
				EmailNotificationRecipients = "emails",
				EnableScheduler = false,
				FieldMappings = "",
				HasErrors = false,
				JobHistory = null,
				LastRuntimeUTC = null,
				LogErrors = false,
				SourceProvider = _sourceProviderId,				
				SourceConfiguration = $"{{ TargetWorkspaceArtifactId : {_targetWorkspaceArtifactId}, SourceWorkspaceArtifactId : {_sourceWorkspaceArtifactId}, SavedSearchArtifactId: {_savedSearchArtifactId} }}",
				NextScheduledRuntimeUTC = null,
				//				OverwriteFields = integrationPoint.OverwriteFields, -- This would require further transformation
				ScheduleRule = String.Empty
			};
			_sourceProvider = new SourceProvider();
			_destinationProvider = new DestinationProvider();
			_integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = _integrationPoint.ArtifactId,
				Name = _integrationPoint.Name,
				DestinationConfiguration = _integrationPoint.DestinationConfiguration,
				DestinationProvider = _integrationPoint.DestinationProvider,
				EmailNotificationRecipients = _integrationPoint.EmailNotificationRecipients,
				EnableScheduler = _integrationPoint.EnableScheduler,
				FieldMappings = _integrationPoint.FieldMappings,
				HasErrors = _integrationPoint.HasErrors,
				JobHistory = _integrationPoint.JobHistory,
				LastRuntimeUTC = _integrationPoint.LastRuntimeUTC,
				LogErrors = _integrationPoint.LogErrors,
				SourceProvider = _integrationPoint.SourceProvider,
				SourceConfiguration = _integrationPoint.SourceConfiguration,
				NextScheduledRuntimeUTC = _integrationPoint.NextScheduledRuntimeUTC,
				//				OverwriteFields = _integrationPoint.OverwriteFields, -- This would require further transformation
				ScheduleRule = _integrationPoint.ScheduleRule
			};
			_previousJobHistory = new Data.JobHistory() {JobStatus = JobStatusChoices.JobHistoryCompleted};
			_stopPermissionChecksResults = new PermissionCheckDTO() {ErrorMessages = new string[0]};

			_integrationPointManager.UserHasPermissionToStopJob(
				_sourceWorkspaceArtifactId,
				 _integrationPoint.ArtifactId)
				.Returns(_stopPermissionChecksResults);

			_jobHistoryManager.GetLastJobHistoryArtifactId(_sourceWorkspaceArtifactId, _integrationPointArtifactId)
				.Returns(_previousJobHistoryArtifactId);
			_caseServiceManager.RsapiService.JobHistoryLibrary.Read(_previousJobHistoryArtifactId).Returns(_previousJobHistory);

			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Read(_integrationPointArtifactId).Returns(_integrationPoint);
			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(_sourceProviderId).Returns(_sourceProvider);
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Read(_destinationProviderId).Returns(_destinationProvider);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_RelativityProvider()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPointManager.UserHasPermissionToRunJob(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
					Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(false);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_GoldFlow()
		{
			// arrange
			int pendingJob1Id = 123;
			int pendingJob2Id = 456;

			int processingJob1Id = 5634;
			int processingJob2Id = 9604;

			var stoppableJobCollection = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new [] { pendingJob1Id, pendingJob2Id },
				ProcessingJobArtifactIds = new [] { processingJob1Id, processingJob2Id }
			};
			_jobHistoryManager
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId), 
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()}; 
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>( x => x.Contains(pendingJob1Id))).Returns(new List<Data.JobHistory>() { pendingJob1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob2Id))).Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob1Id))).Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob2Id))).Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointArtifactId).Returns(jobs);

			// act
			_instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId);

			// assert
			_jobHistoryManager.Received(1)
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId), 
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(2)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.PendingJobArtifactIds.Contains(x.ArtifactId) &&
							x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobArtifactIds.Contains(x.ArtifactId)));

			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>( x => x.Contains(1)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(2)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(3)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(4)));
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_AllQueueUpdatesFail_NoJobStatusesUpdated()
		{
			// arrange
			int pendingJob1Id = 123;
			int pendingJob2Id = 456;

			int processingJob1Id = 5634;
			int processingJob2Id = 9604;

			var stoppableJobCollection = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new[] {pendingJob1Id, pendingJob2Id},
				ProcessingJobArtifactIds = new[] {processingJob1Id, processingJob2Id}
			};
			_jobHistoryManager
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob1Id)))
				.Returns(new List<Data.JobHistory>() {pendingJob1});

			Data.JobHistory pendingJob2 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob2Id)))
				.Returns(new List<Data.JobHistory>() {pendingJob2});

			Data.JobHistory processingJob1 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob1Id)))
				.Returns(new List<Data.JobHistory>() {processingJob1});

			Data.JobHistory processingJob2 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob2Id)))
				.Returns(new List<Data.JobHistory>() {processingJob2});

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(1)};
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(2)};
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(3)};
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(4)};

			_jobManager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointArtifactId).Returns(jobs);

			const string errorMessageOne = "E1";
			const string errorMessageTwo = "E2";
			const string errorMessageThree = "E3";
			const string errorMessageFour = "E4";

			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(1)))).Do(x => {throw new Exception(errorMessageOne);});
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(2)))).Do(x => {throw new Exception(errorMessageTwo);});
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(3)))).Do(x => {throw new Exception(errorMessageThree);});
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(4)))).Do(x => {throw new Exception(errorMessageFour);});

			// act
			bool aggregateExceptionWasThrown = false;
			try
			{
				_instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			}
			catch (AggregateException aggregateException)
			{
				aggregateExceptionWasThrown = true;

				Assert.AreEqual(errorMessageOne, aggregateException.InnerExceptions[0].Message);
				Assert.AreEqual(errorMessageTwo, aggregateException.InnerExceptions[1].Message);
				Assert.AreEqual(errorMessageThree, aggregateException.InnerExceptions[2].Message);
				Assert.AreEqual(errorMessageFour, aggregateException.InnerExceptions[3].Message);
			}
			catch (Exception)
			{
			}

			// assert
			_jobHistoryManager.Received(1)
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<Data.JobHistory>());

			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(1)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(2)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(3)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(4)));

			Assert.IsTrue(aggregateExceptionWasThrown, "An AggregateException was not thrown.");
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_SomeJobsNotMarkedToStop_OnlyMarkedJobsUpdated()
		{
			// arrange
			int pendingJob1Id = 123;
			int pendingJob2Id = 456;

			int processingJob1Id = 5634;
			int processingJob2Id = 9604;

			var stoppableJobCollection = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new[] { pendingJob1Id, pendingJob2Id },
				ProcessingJobArtifactIds = new[] { processingJob1Id, processingJob2Id }
			};
			_jobHistoryManager
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob1Id)))
				.Returns(new List<Data.JobHistory>() { pendingJob1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob2Id)))
				.Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob1Id)))
				.Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob2Id)))
				.Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointArtifactId).Returns(jobs);

			const string errorMessageOne = "E1";
			const string errorMessageTwo = "E2";

			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(1)))).Do(x => { throw new Exception(errorMessageOne); });
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(3)))).Do(x => { throw new Exception(errorMessageTwo); });

			// act
			bool aggregateExceptionWasThrown = false;
			try
			{
				_instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			}
			catch (AggregateException aggregateException)
			{
				aggregateExceptionWasThrown = true;

				Assert.AreEqual(errorMessageOne, aggregateException.InnerExceptions[0].Message);
				Assert.AreEqual(errorMessageTwo, aggregateException.InnerExceptions[1].Message);
			}

			// assert
			_jobHistoryManager.Received(1)
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(1)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							x.ArtifactId == pendingJob2Id
							&& x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobArtifactIds.Contains(x.ArtifactId)));

			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(1)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(2)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(3)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(4)));

			Assert.IsTrue(aggregateExceptionWasThrown, "An AggregateException was not thrown.");
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_OnePendingJobFailsToMarkOnePendingJobsFailsToUpdate_AllExceptionsCaught()
		{
			// arrange
			int pendingJob1Id = 123;
			int pendingJob2Id = 456;

			int processingJob1Id = 5634;
			int processingJob2Id = 9604;

			var stoppableJobCollection = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new[] {pendingJob1Id, pendingJob2Id},
				ProcessingJobArtifactIds = new[] {processingJob1Id, processingJob2Id}
			};
			_jobHistoryManager
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob1Id)))
				.Returns(new List<Data.JobHistory>() {pendingJob1});

			Data.JobHistory pendingJob2 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJob2Id)))
				.Returns(new List<Data.JobHistory>() {pendingJob2});

			Data.JobHistory processingJob1 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob1Id)))
				.Returns(new List<Data.JobHistory>() {processingJob1});

			Data.JobHistory processingJob2 = new Data.JobHistory() {BatchInstance = Guid.NewGuid().ToString()};
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJob2Id)))
				.Returns(new List<Data.JobHistory>() {processingJob2});

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(1)};
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(2)};
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(3)};
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() {baseJob.CopyJobWithJobId(4)};

			_jobManager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointArtifactId).Returns(jobs);

			const string errorMessageOne = "E1";
			const string errorMessageTwo = "E2";
			const string errorMessageThree = "E3";

			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(1))))
				.Do(x => { throw new Exception(errorMessageOne); });
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(3))))
				.Do(x => { throw new Exception(errorMessageTwo); });
			_jobHistoryService.When(x => 
				x.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						y =>
							y.ArtifactId == pendingJob2Id
							&& y.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name)))
				.Do(x => { throw new Exception(errorMessageThree); });

			// act
			bool correctExceptionWasThrown = false;
			try
			{
				_instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			}
			catch (AggregateException aggregateException)
			{
				correctExceptionWasThrown = true;

				Assert.AreEqual(errorMessageOne, aggregateException.InnerExceptions[0].Message);
				Assert.AreEqual(errorMessageTwo, aggregateException.InnerExceptions[1].Message);
				Assert.AreEqual(errorMessageThree, aggregateException.InnerExceptions[2].Message);
			}
			catch (Exception)
			{
			}

			// assert
			_jobHistoryManager.Received(1)
				.GetStoppableJobCollection(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(1)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							x.ArtifactId == pendingJob2Id
							&& x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobArtifactIds.Contains(x.ArtifactId)));

			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(1)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(2)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(3)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(4)));

			Assert.IsTrue(correctExceptionWasThrown, "The correct AggregateException was not thrown.");
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_InsufficientPermission()
		{
			// arrange
			const string errorMessage = " whatever !";
			_stopPermissionChecksResults.ErrorMessages = new[] { errorMessage };

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessage)}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};

			// act
			Exception exception = Assert.Throws<Exception>( () => _instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId));

			// assert
			Assert.IsNotNull(exception);
			Assert.AreEqual(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS, exception.Message);
			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new[] { expectedErrorMessage }, x)));


		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_InvalidPermissions_ThrowsException()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			string[] errorMessages = {"Uh", "oh"};
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { ErrorMessages = errorMessages });

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserIdZero()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_USERID);

			// assert
			_integrationPointManager.Received(0).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_JobsCurrentlyRunning()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);

			_integrationPointManager.UserHasPermissionToRunJob(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
					Arg.Is(Constants.SourceProvider.Relativity))
					.Returns(new PermissionCheckDTO());
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(true);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 12345), Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_queueManager.Received(1).HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			_jobHistoryService.DidNotReceive().GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_destinationProvider.Identifier = "bla bla";

			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO());

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_InvalidPermissions()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";	
			string[] errorMessages = {"Uh", "oh"};
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO() { ErrorMessages = errorMessages});

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 12345), String.Join("<br/>", errorMessages));

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other));

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessages)}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};

			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new [] {expectedErrorMessage}, x)));
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}
		

		[Test]
		public void RetryIntegrationPoint_IntegrationPointHasNoErrors_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPoint.HasErrors = null;
			_integrationPointDto.HasErrors = null;
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Received(1).Read(_integrationPoint.DestinationProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));

			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_InvalidPermissions_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			string[] errorMessages = { "Uh", "oh" };
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { ErrorMessages = errorMessages });

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				String.Join("<br/>", errorMessages));

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Received(1).Read(_integrationPoint.DestinationProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessages)}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};
			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new[] { expectedErrorMessage }, x)));
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_SourceProviderIsNotRelativity_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = "Not a Relativity Provider";

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);

			_integrationPointManager.DidNotReceiveWithAnyArgs().UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_FailToRetrieveJobHistory_NullValue()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());
			_integrationPoint.HasErrors = true;
			_integrationPointDto.HasErrors = true;
			_caseServiceManager.RsapiService.JobHistoryLibrary.Read(_previousJobHistoryArtifactId).Returns((Data.JobHistory)null);

			// Act
			Exception exception = Assert.Throws<Exception>(() =>	_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId));

			// Assert
			Assert.AreEqual("Unable to retrieve the previous job history.", exception.Message);
		}

		[Test]
		public void RetryIntegrationPoint_FailToRetrieveJobHistory_ReceiveException()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());
			_integrationPoint.HasErrors = true;
			_integrationPointDto.HasErrors = true;
			_caseServiceManager.RsapiService.JobHistoryLibrary.Read(_previousJobHistoryArtifactId).Throws<Exception>();

			// Act
			Exception exception = Assert.Throws<Exception>(() => _instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId));

			// Assert
			Assert.AreEqual("Unable to retrieve the previous job history.", exception.Message);
		}


		[Test]
		public void RetryIntegrationPoint_RetryOnStoppedJob()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());
			_integrationPoint.HasErrors = true;
			_integrationPointDto.HasErrors = true;
			_previousJobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;

			// Act
			Exception exception = Assert.Throws<Exception>(() => _instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId));

			// Assert
			Assert.AreEqual(Constants.IntegrationPoints.RETRY_ON_STOPPED_JOB, exception.Message);
		}

		[Test]
		public void RetryIntegrationPoint_GoldFlow_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO());
			_integrationPoint.HasErrors = true;
			_integrationPointDto.HasErrors = true;

			// Act
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
							Arg.Is(_sourceWorkspaceArtifactId),
							Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
							Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GetSourceProvider_ProviderIsNull_ThrowsException_Test()
		{
			// Arrange
			_integrationPoint.SourceProvider = null;

			// Act
			Assert.Throws<Exception>(() =>
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.NO_SOURCE_PROVIDER_SPECIFIED);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.DidNotReceiveWithAnyArgs().Read(Arg.Any<int>());
			_targetPermissionRepository.DidNotReceive().UserCanImport();
			_sourcePermissionRepository.DidNotReceive().UserCanEditDocuments();
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void Update_SourceProviderReadFails_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				LastRun = DateTime.Now
			};

			var existingModel = new IntegrationPointModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration,
				LastRun = model.LastRun
			};

			_instance.When(instance => instance.ReadIntegrationPoint(Arg.Any<int>())).DoNotCallBase();
			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID)).Returns(existingModel);

			const string exceptionMessage = "UH OH!";
			_caseServiceManager.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve source provider");

			// Assert
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
		}

		[Test]
		public void Save_NonPermissionExceptionIsThrown_ExceptionIsWrapped()
		{
			// Arrange
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = 2322 }),
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler() { EnableScheduler = false },
				LastRun = DateTime.Today
			};
			IEnumerable<ErrorDTO> errors = new []
			{
				new ErrorDTO()
				{
					Message = Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
					Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
					WorkspaceId = _sourceWorkspaceArtifactId
				}
			};

			// Act
			const string errorMessage = "KHAAAAAANN!!!";
			var exception = new Exception(errorMessage);
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Read(0).ThrowsForAnyArgs(exception);
			_managerFactory.CreateErrorManager(_contextContainer).Returns(_errorManager);

			// Assert
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), Core.Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE);

			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(
				x => Validate(x.First(), errors.First())));
				//x => x.Equals(errors) && x.First().FullText.Contains("Unable to save Integration Point: Unable to retrieve Integration Point")));
		}

		private bool Validate(ErrorDTO errors1, ErrorDTO errors2)
		{
			bool a = errors1.Equals(errors2);
			bool b = errors1.FullText.Contains("Unable to save Integration Point: Unable to retrieve Integration Point");
			return a && b;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Save_InvalidPermissions_Excepts(bool isRelativityProvider)
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				DestinationProvider = 4242,
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler() { EnableScheduler = false },
				LastRun = null,
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
			};

			var existingModel = new IntegrationPointModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration,
				SelectedOverwrite = model.SelectedOverwrite,
				Scheduler = model.Scheduler,
				LastRun = null
			};

			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID)).Returns(existingModel);
			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<Choice>()
			{
				new Choice(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(Arg.Is(model.SourceProvider))
				.Returns(new SourceProvider()
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Read(Arg.Is(model.DestinationProvider))
				.Returns(new DestinationProvider
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});

			_managerFactory.CreateIntegrationPointManager(_contextContainer)
				.Returns(_integrationPointManager);

			string[] errorMessages = {"Oh", "no"};
			_integrationPointManager.UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO()
				{
					ErrorMessages = errorMessages
				});

			_managerFactory.CreateErrorManager(_contextContainer).Returns(_errorManager);

			var expectedError = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
				FullText = $"{Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{String.Join(Environment.NewLine, errorMessages.Concat(new [] { Constants.IntegrationPoints.NO_USERID }))}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};

			// Act
			Assert.Throws<PermissionException>(() => _instance.SaveIntegration(model), Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE);

			// Assert
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
			_caseServiceManager.RsapiService.DestinationProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.DestinationProvider));
			_integrationPointManager.Received(1).UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other));
			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new ErrorDTO[] {expectedError}, x)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Save_UpdateScenario_NoSchedule_GoldFlow(bool isRelativityProvider)
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				DestinationProvider = 4242,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler() { EnableScheduler = false },
				LastRun = null,
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
			};

			var existingModel = new IntegrationPointModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration,
				DestinationProvider = model.DestinationProvider,
				SelectedOverwrite = model.SelectedOverwrite,
				Scheduler = model.Scheduler,
				LastRun = null
			};

			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID)).Returns(existingModel);
			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<Choice>()
			{
				new Choice(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(Arg.Is(model.SourceProvider))
				.Returns(new SourceProvider()
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Read(Arg.Is(model.DestinationProvider))
				.Returns(new DestinationProvider()
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});

			_managerFactory.CreateIntegrationPointManager(_contextContainer)
				.Returns(_integrationPointManager);

			_integrationPointManager.UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO());

			_caseServiceManager.EddsUserID = 1232;

			// Act
			int result = _instance.SaveIntegration(model);

			// Assert
			Assert.AreEqual(model.ArtifactID, result, "The resulting artifact id should match.");
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
			_caseServiceManager.RsapiService.DestinationProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.DestinationProvider));
			_integrationPointManager.Received(1).UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other));
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Received(1).Update(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == model.ArtifactID));
			_jobManager.Received(1).GetJob(
				_sourceWorkspaceArtifactId, 
				model.ArtifactID,
				isRelativityProvider ? TaskType.ExportService.ToString() : TaskType.SyncManager.ToString());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Save_CreateScenario_NoSchedule_GoldFlow(bool isRelativityProvider)
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 0,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				DestinationProvider = 4242,
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler() { EnableScheduler = false },
				LastRun = null,
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
			};

			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<Choice>()
			{
				new Choice(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(Arg.Is(model.SourceProvider))
				.Returns(new SourceProvider()
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Read(Arg.Is(model.DestinationProvider))
				.Returns(new DestinationProvider
				{
					Identifier = isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID : System.Guid.NewGuid().ToString()
				});

			_managerFactory.CreateIntegrationPointManager(_contextContainer)
				.Returns(_integrationPointManager);

			_integrationPointManager.UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO());

			const int newIntegrationPoinId = 389234;
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Create(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == 0))
				.Returns(newIntegrationPoinId);

			_caseServiceManager.EddsUserID = 1232;

			// Act
			int result = _instance.SaveIntegration(model);

			// Assert
			Assert.AreEqual(newIntegrationPoinId, result, "The resulting artifact id should match.");
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
			_caseServiceManager.RsapiService.DestinationProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.DestinationProvider));
			_integrationPointManager.Received(1).UserHasPermissionToSaveIntegrationPoint(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
				Arg.Is(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other));
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Received(1).Create(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == newIntegrationPoinId));
			_jobManager.Received(1).GetJob(
				_sourceWorkspaceArtifactId,
				newIntegrationPoinId,
				isRelativityProvider ? TaskType.ExportService.ToString() : TaskType.SyncManager.ToString());
		}

		[Test]
		public void Update_IPReadFails_Excepts()
		{
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
			};

			const string exceptionMessage = "UH OH!";
			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve Integration Point");

			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
		}

		[Test]
		public void SaveIntegration_MakeSureToCreateAJobWithNoBatchInstanceId()
		{
			// arrange
			const int targetWorkspaceArtifactId = 9302;
			const int integrationPointArtifactId = 9847654;
			_caseServiceManager.EddsUserID = 78946;
			_caseServiceManager.WorkspaceID = _sourceWorkspaceArtifactId;
			var model = new IntegrationPointModel()
			{
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				DestinationProvider = 4242,
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
					EndDate = DateTime.Now.AddDays(1).ToString(CultureInfo.InvariantCulture),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					Reoccur = 2,
				},
				LastRun = null,
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = ""})
			};
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Create(Arg.Any<Data.IntegrationPoint>())
				.Returns(integrationPointArtifactId);

			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<Choice>()
			{
				new Choice(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(Arg.Is(model.SourceProvider))
				.Returns(new SourceProvider()
				{
					Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID
				});
			_caseServiceManager.RsapiService.DestinationProviderLibrary.Read(Arg.Is(model.DestinationProvider))
				.Returns(new DestinationProvider()
				{
					Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID
				});

			_integrationPointManager.UserHasPermissionToSaveIntegrationPoint(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is<IntegrationPointDTO>(x => x.ArtifactId == model.ArtifactID),
					Arg.Is(Constants.SourceProvider.Relativity))
					.Returns(new PermissionCheckDTO());

			// Act
			int ipArtifactId = _instance.SaveIntegration(model);

			// Assert
			_jobManager.Received(1).CreateJob<TaskParameters>(null, TaskType.ExportService, _caseServiceManager.WorkspaceID, ipArtifactId, Arg.Any<IScheduleRule>());
		}

		[Test]
		[TestCase(false, new string[] { "Name" })]
		[TestCase(false, new string[] { "Destination Provider" })]
		[TestCase(false, new string[] { "Destination RDO" })]
		[TestCase(false, new string[] { "Case" })]
		[TestCase(false, new string[] { "Source Provider" })]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case" })]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Provider" })]
		[TestCase(false, new string[] { "Name", "Source Configuration" })] // normal providers will only throw with "Name" in list
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" })]
		[TestCase(true, new string[] { "Source Configuration" })]
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" })] // If relativity provider and no permissions, throw permissions error first
		public void Update_InvalidProperties_Excepts(bool isRelativityProvider, string[] propertyNames)
		{
			// Arrange
			var propertyNameHashSet = new HashSet<string>(propertyNames);
			const int targetWorkspaceArtifactId = 12329;
			const int sourceWorkspaceArtifactId = 92321;
			int existingTargetWorkspaceArtifactId = propertyNameHashSet.Contains("Source Configuration")
				? 12324
				: targetWorkspaceArtifactId;
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				Name = "My Name",
				DestinationProvider = 4909,
				SourceProvider = 9830,
				Destination = JsonConvert.SerializeObject(new { artifactTypeID = 10, CaseArtifactId = 7891232 }),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				})
			};

			var existingModel = new IntegrationPointModel()
			{
				ArtifactID = model.ArtifactID,
				LastRun = DateTime.Now,
				Name = propertyNameHashSet.Contains("Name") ? "Diff Name" : model.Name,
				DestinationProvider = propertyNameHashSet.Contains("Destination Provider") ? 12343 : model.DestinationProvider,
				SourceProvider = propertyNameHashSet.Contains("Source Provider") ? 391232 : model.SourceProvider,
				Destination = JsonConvert.SerializeObject(new
				{
					artifactTypeID = propertyNameHashSet.Contains("Destination RDO") ? 13 : 10,
					CaseArtifactId = propertyNameHashSet.Contains("Case") ? 18392 : 7891232
				}),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = existingTargetWorkspaceArtifactId
				})
			};

			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			// Source Provider is special, if this changes we except earlier
			if (!propertyNameHashSet.Contains("Source Provider"))
			{
				var sourceProvider = new SourceProvider()
				{
					Identifier = isRelativityProvider
						? Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID
						: "YODUDE"
				};
				_caseServiceManager.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Returns(sourceProvider);
			}

			string filteredNames = String.Join(",", propertyNames.Where(x => isRelativityProvider || x != "Source Configuration").Select(x => $" {x}"));
			string expectedErrorString = $"Unable to save Integration Point:{filteredNames} cannot be changed once the Integration Point has been run";

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), expectedErrorString);

			// Assert
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(!propertyNameHashSet.Contains("Source Provider") ? 1 : 0)
				.Read(Arg.Is(model.SourceProvider));
		}

		[Test]
		public void GetRdo_ArtifactIdExists_ReturnsRdo_Test()
		{
			//Act
			Data.IntegrationPoint integrationPoint = _instance.GetRdo(_integrationPointArtifactId);

			//Assert
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Received(1).Read(_integrationPointArtifactId);
			Assert.IsNotNull(integrationPoint);
		}

		[Test]
		public void GetRdo_ArtifactIdDoesNotExist_ExceptionThrown_Test()
		{
			//Arrange
			_caseServiceManager.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Read(_integrationPointArtifactId).Throws<Exception>();

			//Act
			Assert.Throws<Exception>(() => _instance.GetRdo(_integrationPointArtifactId), "Unable to retrieve Integration Point.");
		}
	}
}