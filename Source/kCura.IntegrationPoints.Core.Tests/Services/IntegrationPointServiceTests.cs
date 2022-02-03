using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
	[TestFixture, Category("Unit")]
	public class IntegrationPointServiceTests : TestBase
	{
		private Data.IntegrationPoint _integrationPoint;
		private Data.JobHistory _previousJobHistory;
		private DestinationProvider _destinationProvider;
		private ICaseServiceContext _caseServiceContext;
		private IRelativityObjectManager _objectManager;
		private IChoiceQuery _choiceQuery;
		private IErrorManager _errorManager;
		private IHelper _helper;
		private IIntegrationPointRepository _integrationPointRepository;
		private IIntegrationPointSerializer _serializer;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IJobHistoryManager _jobHistoryManager;
		private IJobHistoryService _jobHistoryService;
		private IJobManager _jobManager;
		private IManagerFactory _managerFactory;
		private IMessageService _messageService;
		private IntegrationPointModelBase _integrationPointModel;
		private IntegrationPointService _instance;
		private IntegrationPointType _integrationPointType;
		private IPermissionRepository _sourcePermissionRepository;
		private IPermissionRepository _targetPermissionRepository;
		private IProviderTypeService _providerTypeService;
		private IQueueManager _queueManager;
		private IRepositoryFactory _repositoryFactory;
		private IValidationExecutor _validationExecutor;
		private SourceProvider _sourceProvider;
		private ValidationResult _stopPermissionChecksResults;
		private ITaskParametersBuilder _taskParametersBuilder;

		private readonly int _destinationProviderId = 424;
		private readonly int _integrationPointArtifactId = 741;
		private readonly int _integrationPointTypeArtifactId = 12345;
		private readonly int _previousJobHistoryArtifactId = Int32.MaxValue;
		private readonly int _savedSearchArtifactId = 93032;
		private readonly int _sourceProviderId = 321;
		private readonly int _sourceWorkspaceArtifactId = 789;
		private readonly int _targetWorkspaceArtifactId = 9954;
		private readonly int _userId = 951;
		private readonly Guid _objectTypeGuid = ObjectTypeGuids.IntegrationPointGuid;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_objectManager = Substitute.For<IRelativityObjectManager>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();
			_targetPermissionRepository = Substitute.For<IPermissionRepository>();
			_serializer = Substitute.For<IIntegrationPointSerializer>();
			_jobManager = Substitute.For<IJobManager>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_queueManager = Substitute.For<IQueueManager>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_errorManager = Substitute.For<IErrorManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_providerTypeService = Substitute.For<IProviderTypeService>();
			_messageService = Substitute.For<IMessageService>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_taskParametersBuilder = Substitute.For<ITaskParametersBuilder>();

			_validationExecutor = Substitute.For<IValidationExecutor>();

			_instance = Substitute.ForPartsOf<IntegrationPointService>(
				_helper,
				_caseServiceContext,
				_serializer,
				_choiceQuery,
				_jobManager,
				_jobHistoryService,
				_jobHistoryErrorService,
				_managerFactory,
				_validationExecutor,
				_providerTypeService,
				_messageService,
				_integrationPointRepository,
				_objectManager,
				_taskParametersBuilder
			);

			_caseServiceContext.RelativityObjectManagerService = Substitute.For<IRelativityObjectManagerService>();
			_caseServiceContext.WorkspaceID = _sourceWorkspaceArtifactId;

			_repositoryFactory.GetPermissionRepository(_sourceWorkspaceArtifactId).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(_targetWorkspaceArtifactId).Returns(_targetPermissionRepository);
			_managerFactory.CreateErrorManager().Returns(_errorManager);
			_managerFactory.CreateJobHistoryManager().Returns(_jobHistoryManager);

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
				OverwriteFields = new ChoiceRef(1000) { Name = "AppendOnly" },
				ScheduleRule = String.Empty,
				Type = _integrationPointTypeArtifactId,
				PromoteEligible = false,
				SecuredConfiguration = string.Empty
			};
			_sourceProvider = new SourceProvider();
			_destinationProvider = new DestinationProvider();
			_integrationPointType = new IntegrationPointType();

			_previousJobHistory = new Data.JobHistory() { JobStatus = JobStatusChoices.JobHistoryCompleted };
			_stopPermissionChecksResults = new ValidationResult();

			_integrationPointModel = IntegrationPointModel.FromIntegrationPoint(_integrationPoint);

			var context = new ValidationContext
			{
				DestinationProvider = _destinationProvider,
				IntegrationPointType = _integrationPointType,
				Model = Arg.Is<IntegrationPointModelBase>(x => MatchHelper.Matches(_integrationPointModel, x)),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
				SourceProvider = _sourceProvider,
				UserId = -1
			};

			_validationExecutor.ValidateOnStop(context);

			_jobHistoryManager.GetLastJobHistoryArtifactId(_sourceWorkspaceArtifactId, _integrationPointArtifactId)
				.Returns(_previousJobHistoryArtifactId);
			_objectManager.Query<Data.JobHistory>(Arg.Is<QueryRequest>(q => q.Condition.Contains(_previousJobHistoryArtifactId.ToString())))
				.Returns(new List<Data.JobHistory>
				{
					_previousJobHistory
				});

			_integrationPointRepository.ReadWithFieldMappingAsync(_integrationPointArtifactId).Returns(_integrationPoint);
			_objectManager.Read<SourceProvider>(_sourceProviderId).Returns(_sourceProvider);
			_objectManager.Read<DestinationProvider>(_destinationProviderId).Returns(_destinationProvider);
			_integrationPointRepository.ReadWithFieldMappingAsync(_integrationPointArtifactId).Returns(_integrationPoint);
			_objectManager.Read<IntegrationPointType>(_integrationPointTypeArtifactId).Returns(_integrationPointType);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_RelativityProvider()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(false);

			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
			_managerFactory.Received().CreateQueueManager();
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_GoldFlow()
		{
			// arrange
			var pendingJobHistory1 = new Data.JobHistory { ArtifactId = 123 };
			var pendingJobHistory2 = new Data.JobHistory { ArtifactId = 456 };

			var processingJobHistory1 = new Data.JobHistory { ArtifactId = 5634 };
			var processingJobHistory2 = new Data.JobHistory { ArtifactId = 9604 };

			var stoppableJobCollection = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { pendingJobHistory1, pendingJobHistory2 },
				ProcessingJobHistory = new[] { processingJobHistory1, processingJobHistory2 }
			};
			_jobHistoryManager
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory1.ArtifactId))).Returns(new List<Data.JobHistory>() { pendingJobHistory1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory2.ArtifactId))).Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory1.ArtifactId))).Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory2.ArtifactId))).Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJobHistory1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetJobsByBatchInstanceId(_integrationPointArtifactId).Returns(jobs);

			// act
			_instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId);

			// assert
			_jobHistoryManager.Received(1)
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(2)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.PendingJobHistory.Contains(x) &&
							x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobHistory.Contains(x)));

			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(1)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(2)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(3)));
			_jobManager.Received(1).StopJobs(Arg.Is<List<long>>(x => x.Contains(4)));
		}

		[Test]
		public void MarkIntegrationPointToStopJobs_AllQueueUpdatesFail_NoJobStatusesUpdated()
		{
			// arrange
			var pendingJobHistory1 = new Data.JobHistory { ArtifactId = 123 };
			var pendingJobHistory2 = new Data.JobHistory { ArtifactId = 456 };

			var processingJobHistory1 = new Data.JobHistory { ArtifactId = 5634 };
			var processingJobHistory2 = new Data.JobHistory { ArtifactId = 9604 };

			var stoppableJobCollection = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { pendingJobHistory1, pendingJobHistory2 },
				ProcessingJobHistory = new[] { processingJobHistory1, processingJobHistory2 }
			};
			_jobHistoryManager
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetJobsByBatchInstanceId(_integrationPointArtifactId).Returns(jobs);

			const string errorMessageOne = "E1";
			const string errorMessageTwo = "E2";
			const string errorMessageThree = "E3";
			const string errorMessageFour = "E4";

			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(1)))).Do(x => { throw new Exception(errorMessageOne); });
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(2)))).Do(x => { throw new Exception(errorMessageTwo); });
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(3)))).Do(x => { throw new Exception(errorMessageThree); });
			_jobManager.When(x => x.StopJobs(Arg.Is<List<long>>(y => y.Contains(4)))).Do(x => { throw new Exception(errorMessageFour); });

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
				.GetStoppableJobHistory(
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
			var pendingJobHistory1 = new Data.JobHistory { ArtifactId = 123 };
			var pendingJobHistory2 = new Data.JobHistory { ArtifactId = 456 };

			var processingJobHistory1 = new Data.JobHistory { ArtifactId = 5634 };
			var processingJobHistory2 = new Data.JobHistory { ArtifactId = 9604 };

			var stoppableJobCollection = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { pendingJobHistory1, pendingJobHistory2 },
				ProcessingJobHistory = new[] { processingJobHistory1, processingJobHistory2 }
			};
			_jobHistoryManager
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetJobsByBatchInstanceId(_integrationPointArtifactId).Returns(jobs);

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
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(1)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							x.ArtifactId == pendingJobHistory2.ArtifactId
							&& x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobHistory.Contains(x)));

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
			var pendingJobHistory1 = new Data.JobHistory { ArtifactId = 123 };
			var pendingJobHistory2 = new Data.JobHistory { ArtifactId = 456 };

			var processingJobHistory1 = new Data.JobHistory { ArtifactId = 5634 };
			var processingJobHistory2 = new Data.JobHistory { ArtifactId = 9604 };

			var stoppableJobCollection = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { pendingJobHistory1, pendingJobHistory2 },
				ProcessingJobHistory = new[] { processingJobHistory1, processingJobHistory2 }
			};

			_jobHistoryManager
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId))
				.Returns(stoppableJobCollection);

			Data.JobHistory pendingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob1 });

			Data.JobHistory pendingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(pendingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { pendingJob2 });

			Data.JobHistory processingJob1 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory1.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob1 });

			Data.JobHistory processingJob2 = new Data.JobHistory() { BatchInstance = Guid.NewGuid().ToString() };
			_jobHistoryService.GetJobHistory(Arg.Is<List<int>>(x => x.Contains(processingJobHistory2.ArtifactId)))
				.Returns(new List<Data.JobHistory>() { processingJob2 });

			Job baseJob = JobExtensions.CreateJob();
			IDictionary<Guid, List<Job>> jobs = new Dictionary<Guid, List<Job>>();
			jobs[new Guid(pendingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(1) };
			jobs[new Guid(pendingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(2) };
			jobs[new Guid(processingJob1.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(3) };
			jobs[new Guid(processingJob2.BatchInstance)] = new List<Job>() { baseJob.CopyJobWithJobId(4) };

			_jobManager.GetJobsByBatchInstanceId(_integrationPointArtifactId).Returns(jobs);

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
							y.ArtifactId == pendingJobHistory2.ArtifactId
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
				.GetStoppableJobHistory(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is(_integrationPointArtifactId));

			_jobHistoryService.Received(1)
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							x.ArtifactId == pendingJobHistory2.ArtifactId
							&& x.JobStatus.Name == JobStatusChoices.JobHistoryStopping.Name));

			_jobHistoryService.DidNotReceive()
				.UpdateRdo(
					Arg.Is<Data.JobHistory>(
						x =>
							stoppableJobCollection.ProcessingJobHistory.Contains(x)));

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
			_stopPermissionChecksResults.Add(errorMessage);

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessage)}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};

			_validationExecutor
				.When(mock => mock.ValidateOnStop(Arg.Any<ValidationContext>()))
				.Do(x => { throw new PermissionException(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS); });

			// act
			Exception exception = Assert.Throws<PermissionException>(() => _instance.MarkIntegrationPointToStopJobs(_sourceWorkspaceArtifactId, _integrationPointArtifactId));

			// assert
			Assert.IsNotNull(exception);
			Assert.AreEqual(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS, exception.Message);
			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new[] { expectedErrorMessage }, x)));
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_InvalidPermissions_ThrowsException()
		{
			// arrange
			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_validationExecutor
				.When(mock => mock.ValidateOnRun(Arg.Is<ValidationContext>(x =>
					x.IntegrationPointType == _integrationPointType &&
					x.SourceProvider == _sourceProvider &&
					x.UserId == _userId &&
					x.DestinationProvider == _destinationProvider &&
					MatchHelper.Matches(_integrationPointModel, x.Model) &&
					x.ObjectTypeGuid == _objectTypeGuid)))
				.Do(x => { throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE); });

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId));

			// assert

			_validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

			_jobHistoryService.DidNotReceive().GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);


			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());

			// Check if Job History Error has benn created
			_jobHistoryErrorService.Received().AddError(Arg.Is<ChoiceRef>(x => x.Name == ErrorTypeChoices.JobHistoryErrorJob.Name), string.Empty, Arg.Any<string>(), string.Empty);
			// Check if Job status changed to Validation Failed
			_jobHistoryService.Received().UpdateRdo(Arg.Is<Data.JobHistory>(x => x.JobStatus.Name == JobStatusChoices.JobHistoryValidationFailed.Name));
			// Check If Integration Points objest has been set HasErrors flag to YES
			_integrationPointRepository.Received()
				.Update(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == _integrationPointArtifactId && x.HasErrors == true));
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_JobsCurrentlyRunning()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_managerFactory.CreateQueueManager().Returns(_queueManager);

			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(true);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);

			// assert

			_validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

			_queueManager.Received(1).HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			_jobHistoryService.DidNotReceive().GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
			_managerFactory.Received().CreateQueueManager();
		}

		[Test]
		public void RetryIntegrationPoint_IntegrationPointHasNoErrors_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPoint.HasErrors = null;

			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);

			// Assert
			_objectManager.Received(1).Read<SourceProvider>(_integrationPoint.SourceProvider.Value);
			_objectManager.Received(1).Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value);

			_validationExecutor.DidNotReceive().ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_InvalidPermissions_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPoint.HasErrors = true;
			_jobHistoryService.CreateRdo(
							Arg.Any<Data.IntegrationPoint>(),
							Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
							Arg.Any<DateTime?>())
						.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			_validationExecutor
				.When(mock => mock.ValidateOnRun(Arg.Is<ValidationContext>(x =>
					x.IntegrationPointType == _integrationPointType &&
					x.SourceProvider == _sourceProvider &&
					x.UserId == _userId &&
					x.DestinationProvider == _destinationProvider &&
					MatchHelper.Matches(_integrationPointModel, x.Model) &&
					x.ObjectTypeGuid == _objectTypeGuid)))
				.Do(x => { throw new Exception(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE); });

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				String.Join("<br/>", Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE));

			// Assert
			_objectManager.Received(1).Read<SourceProvider>(_integrationPoint.SourceProvider.Value);
			_objectManager.Received(1).Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value);

			_validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

			_jobHistoryErrorService.Received().AddError(Arg.Is<ChoiceRef>(x => x.Name == ErrorTypeChoices.JobHistoryErrorJob.Name), string.Empty, Arg.Any<string>(), string.Empty);
			_jobHistoryService.Received().UpdateRdo(Arg.Is<Data.JobHistory>(x => x.JobStatus.Name == JobStatusChoices.JobHistoryValidationFailed.Name));

			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);

			_integrationPointRepository.Received()
					.Update(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == _integrationPointArtifactId && x.HasErrors == true));
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
			_objectManager.Received(1).Read<SourceProvider>(_integrationPoint.SourceProvider.Value);

			_validationExecutor.DidNotReceiveWithAnyArgs().ValidateOnRun(Arg.Any<ValidationContext>());

			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_FailToRetrieveJobHistory_NullValue()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPoint.HasErrors = true;
			_objectManager.Query<Data.JobHistory>(Arg.Is<QueryRequest>(q => q.Condition.Contains(_previousJobHistoryArtifactId.ToString())))
				.Returns((List<Data.JobHistory>)null);

			// Act
			Exception exception = Assert.Throws<Exception>(() => _instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId));

			// Assert
			Assert.AreEqual("Unable to retrieve the previous job history.", exception.Message);
		}

		[Test]
		public void RetryIntegrationPoint_FailToRetrieveJobHistory_ReceiveException()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_destinationProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;

			_integrationPoint.HasErrors = true;
			_objectManager.Query<Data.JobHistory>(Arg.Is<QueryRequest>(q => q.Condition.Contains(_previousJobHistoryArtifactId.ToString())))
				.Throws<Exception>();

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

			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			_integrationPoint.HasErrors = true;
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

			_integrationPoint.HasErrors = true;

			_jobHistoryService.CreateRdo(
					Arg.Any<Data.IntegrationPoint>(),
					Arg.Any<Guid>(), Arg.Any<ChoiceRef>(),
					Arg.Any<DateTime?>())
				.Returns(new Data.JobHistory() { BatchInstance = string.Empty });

			// Act
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// Assert
			_objectManager.Received(1).Read<SourceProvider>(_integrationPoint.SourceProvider.Value);

			_validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == _integrationPointType &&
				x.SourceProvider == _sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == _destinationProvider &&
				MatchHelper.Matches(_integrationPointModel, x.Model) &&
				x.ObjectTypeGuid == _objectTypeGuid)
			);

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
			_objectManager.DidNotReceiveWithAnyArgs().Read<SourceProvider>(Arg.Any<int>());
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

			_instance.When(instance => instance.ReadIntegrationPointModel(Arg.Any<int>())).DoNotCallBase();
			_instance.ReadIntegrationPointModel(Arg.Is(model.ArtifactID)).Returns(existingModel);

			const string exceptionMessage = "UH OH!";
			_objectManager
					.Read<SourceProvider>(Arg.Is(model.SourceProvider))
					.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve source provider");

			// Assert
			_instance.Received(1).ReadIntegrationPointModel(Arg.Is(model.ArtifactID));
			_objectManager
				.Received(1)
				.Read<SourceProvider>(Arg.Is(model.SourceProvider));
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
			IEnumerable<ErrorDTO> errors = new[]
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
			_integrationPointRepository.ReadWithFieldMappingAsync(0).ThrowsForAnyArgs(exception);
			_managerFactory.CreateErrorManager().Returns(_errorManager);

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

			_instance.ReadIntegrationPointModel(Arg.Is(model.ArtifactID)).Returns(existingModel);
			_choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>()
			{
				new ChoiceRef(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			var sourceProvider = new SourceProvider()
			{
				Identifier =
					isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
			};

			var destinationProvider = new DestinationProvider
			{
				Identifier =
					isRelativityProvider
						? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID
						: System.Guid.NewGuid().ToString()
			};

			var integrationPointType = new IntegrationPointType()
			{
				Identifier =
					isRelativityProvider
						? Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()
						: System.Guid.NewGuid().ToString()
			};

			_objectManager.Read<SourceProvider>(Arg.Is(model.SourceProvider)).Returns(sourceProvider);
			_objectManager.Read<DestinationProvider>(Arg.Is(model.DestinationProvider)).Returns(destinationProvider);
			_objectManager.Read<IntegrationPointType>(Arg.Is(model.Type)).Returns(integrationPointType);

			string[] errorMessages = { "Oh", "no" };

			_caseServiceContext.EddsUserID = _userId;

			_validationExecutor
				.When(mock => mock.ValidateOnSave(Arg.Is<ValidationContext>(x =>
					x.IntegrationPointType == integrationPointType &&
					x.SourceProvider == sourceProvider &&
					x.UserId == _userId &&
					x.DestinationProvider == destinationProvider &&
					x.Model.ArtifactID == model.ArtifactID &&
					x.ObjectTypeGuid == _objectTypeGuid

						)))
				.Do(x => { throw new PermissionException(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE); });

			_managerFactory.CreateErrorManager().Returns(_errorManager);

			var expectedError = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
				FullText = $"{Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{String.Join(Environment.NewLine, errorMessages.Concat(new[] { Constants.IntegrationPoints.NO_USERID }))}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _sourceWorkspaceArtifactId
			};

			// Act
			Assert.Throws<PermissionException>(() => _instance.SaveIntegration(model), Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE);

			// Assert
			_instance.Received(1).ReadIntegrationPointModel(Arg.Is(model.ArtifactID));
			_objectManager
				.Received(1)
				.Read<SourceProvider>(Arg.Is(model.SourceProvider));
			_objectManager
				.Received(1)
				.Read<DestinationProvider>(Arg.Is(model.DestinationProvider));

			_validationExecutor.Received(1).ValidateOnSave(Arg.Is<ValidationContext>(x =>
					x.IntegrationPointType == integrationPointType &&
					x.SourceProvider == sourceProvider &&
					x.UserId == _userId &&
					x.DestinationProvider == destinationProvider &&
					x.Model.ArtifactID == model.ArtifactID &&
					x.ObjectTypeGuid == _objectTypeGuid
			));

			_errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new ErrorDTO[] { expectedError }, x)));
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

			_instance.ReadIntegrationPointModel(Arg.Is(model.ArtifactID)).Returns(existingModel);
			_choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>()
			{
				new ChoiceRef(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			var sourceProvider = new SourceProvider()
			{
				Identifier =
					isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
			};

			var destinationProvider = new DestinationProvider
			{
				Identifier =
					isRelativityProvider
						? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID
						: System.Guid.NewGuid().ToString()
			};

			var integrationPointType = new IntegrationPointType()
			{
				Identifier =
					isRelativityProvider
						? Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()
						: System.Guid.NewGuid().ToString()
			};

			_objectManager.Read<SourceProvider>(Arg.Is(model.SourceProvider)).Returns(sourceProvider);
			_objectManager.Read<DestinationProvider>(Arg.Is(model.DestinationProvider)).Returns(destinationProvider);
			_objectManager.Read<IntegrationPointType>(Arg.Is(model.Type)).Returns(integrationPointType);

			_caseServiceContext.EddsUserID = _userId;

			_integrationPointRepository
				.CreateOrUpdate(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == model.ArtifactID))
				.Returns(model.ArtifactID);

			// Act
			int result = _instance.SaveIntegration(model);

			// Assert
			Assert.AreEqual(model.ArtifactID, result, "The resulting artifact id should match.");
			_instance.Received(1).ReadIntegrationPointModel(Arg.Is(model.ArtifactID));
			_objectManager
				.Received(1)
				.Read<SourceProvider>(Arg.Is(model.SourceProvider));
			_objectManager
				.Received(1)
				.Read<DestinationProvider>(Arg.Is(model.DestinationProvider));

			_validationExecutor.Received(1).ValidateOnSave(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == integrationPointType &&
				x.SourceProvider == sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == destinationProvider &&
				x.Model.ArtifactID == model.ArtifactID &&
				x.ObjectTypeGuid == _objectTypeGuid
			));

			_integrationPointRepository
				.Received(1)
				.CreateOrUpdate(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == model.ArtifactID));
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

			_choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>()
			{
				new ChoiceRef(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			var sourceProvider = new SourceProvider()
			{
				Identifier =
					isRelativityProvider ? Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID : System.Guid.NewGuid().ToString()
			};

			var destinationProvider = new DestinationProvider
			{
				Identifier =
					isRelativityProvider
						? Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID
						: System.Guid.NewGuid().ToString()
			};

			var integrationPointType = new IntegrationPointType()
			{
				Identifier =
					isRelativityProvider
						? Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()
						: System.Guid.NewGuid().ToString()
			};

			_objectManager.Read<SourceProvider>(Arg.Is(model.SourceProvider)).Returns(sourceProvider);
			_objectManager.Read<DestinationProvider>(Arg.Is(model.DestinationProvider)).Returns(destinationProvider);
			_objectManager.Read<IntegrationPointType>(Arg.Is(model.Type)).Returns(integrationPointType);

			const int newIntegrationPoinId = 389234;
			_integrationPointRepository
				.CreateOrUpdate(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == 0))
				.Returns(newIntegrationPoinId);

			_caseServiceContext.EddsUserID = _userId;

			// Act
			int result = _instance.SaveIntegration(model);

			// Assert
			Assert.AreEqual(newIntegrationPoinId, result, "The resulting artifact id should match.");
			_objectManager
				.Received(1)
				.Read<SourceProvider>(Arg.Is(model.SourceProvider));
			_objectManager
				.Received(1)
				.Read<DestinationProvider>(Arg.Is(model.DestinationProvider));

			_validationExecutor.Received(1).ValidateOnSave(Arg.Is<ValidationContext>(x =>
				x.IntegrationPointType == integrationPointType &&
				x.SourceProvider == sourceProvider &&
				x.UserId == _userId &&
				x.DestinationProvider == destinationProvider &&
				x.Model.ArtifactID == model.ArtifactID &&
				x.ObjectTypeGuid == _objectTypeGuid
			));

			_integrationPointRepository.Received(1).CreateOrUpdate(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == newIntegrationPoinId));
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
			_instance.ReadIntegrationPointModel(Arg.Is(model.ArtifactID))
				.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve Integration Point");

			_instance.Received(1).ReadIntegrationPointModel(Arg.Is(model.ArtifactID));
		}

		[Test]
		public void SaveIntegration_MakeSureToCreateAJobWithBatchInstanceId()
		{
			// arrange
			const int targetWorkspaceArtifactId = 9302;
			const int integrationPointArtifactId = 9847654;
			_caseServiceContext.EddsUserID = 78946;
			_caseServiceContext.WorkspaceID = _sourceWorkspaceArtifactId;
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
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
			};
			_integrationPointRepository.CreateOrUpdate(Arg.Any<Data.IntegrationPoint>())
				.Returns(integrationPointArtifactId);

			_choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>()
			{
				new ChoiceRef(2343)
				{
					Name = model.SelectedOverwrite
				}
			});

			var sourceProvider = new SourceProvider()
			{
				Identifier = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID
			};

			var destinationProvider = new DestinationProvider
			{
				Identifier = Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID
			};

			_objectManager.Read<SourceProvider>(Arg.Is(model.SourceProvider)).Returns(sourceProvider);
			_objectManager.Read<DestinationProvider>(Arg.Is(model.DestinationProvider)).Returns(destinationProvider);

			// Act
			int ipArtifactId = _instance.SaveIntegration(model);

			// Assert
			_jobManager.Received(1).CreateJob<TaskParameters>(Arg.Any<TaskParameters>(), TaskType.ExportService, _caseServiceContext.WorkspaceID, ipArtifactId, Arg.Any<IScheduleRule>());
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

			_instance.ReadIntegrationPointModel(Arg.Is(model.ArtifactID))
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
				_objectManager
					.Read<SourceProvider>(Arg.Is(model.SourceProvider))
					.Returns(sourceProvider);
			}

			string filteredNames = String.Join(",", propertyNames.Where(x => isRelativityProvider || x != "Source Configuration").Select(x => $" {x}"));
			string expectedErrorString = $"Unable to save Integration Point:{filteredNames} cannot be changed once the Integration Point has been run";

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), expectedErrorString);

			// Assert
			_instance.Received(1).ReadIntegrationPointModel(Arg.Is(model.ArtifactID));
			_objectManager
				.Received(!propertyNameHashSet.Contains("Source Provider") ? 1 : 0)
				.Read<SourceProvider>(Arg.Is(model.SourceProvider));
		}

		[Test]
		public void ReadIntegrationPointModel_ShouldReturnIntegrationPointModel()
		{
			// arrange
			_integrationPointRepository.ReadWithFieldMappingAsync(_integrationPointArtifactId).Returns(Task.FromResult(_integrationPoint));
			IntegrationPointModel expectedResult = (IntegrationPointModel) _integrationPointModel;

			// act
			IntegrationPointModel result = _instance.ReadIntegrationPointModel(_integrationPointArtifactId);

			// assert
			_integrationPointRepository.Received(1).ReadWithFieldMappingAsync(_integrationPointArtifactId);
			MatchHelper.Matches(expectedResult, result);
		}

		[Test]
		public void ReadIntegrationPoint_ShouldReturnIntegrationPoint_WhenRepositoryReturnsIntegrationPoint()
		{
			// arrange
			_integrationPointRepository.ReadWithFieldMappingAsync(_integrationPointArtifactId).Returns(Task.FromResult(_integrationPoint));

			// act
			Data.IntegrationPoint result = _instance.ReadIntegrationPoint(_integrationPointArtifactId);

			// assert
			_integrationPointRepository.Received(1).ReadWithFieldMappingAsync(_integrationPointArtifactId);
			MatchHelper.Matches(_integrationPoint, result);
		}

		[Test]
		public void ReadIntegrationPoint_ShouldThrowException_WhenRepositoryThrowsException()
		{
			// arrange
			_integrationPointRepository.ReadWithFieldMappingAsync(_integrationPointArtifactId).Throws<Exception>();

			// act
			Assert.Throws<Exception>(() =>_instance.ReadIntegrationPoint(_integrationPointArtifactId));

			// assert
			_integrationPointRepository.Received(1).ReadWithFieldMappingAsync(_integrationPointArtifactId);
		}
	}
}