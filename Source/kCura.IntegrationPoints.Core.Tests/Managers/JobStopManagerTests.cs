using System;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture, Category("Unit")]
	public class JobStopManagerTests : TestBase
	{
		private JobStopManager _instance;
		private Mock<IJobService> _jobService;
		private Mock<IJobHistoryService> _jobHistoryService;
		private Mock<IJobServiceDataProvider> _jobServiceDataProvider;
		private Mock<IRemovableAgent> _agent;
		private Mock<IHelper> _helper;
		private Guid _jobHistoryInstanceGuid;
		private int _jobId;
		private JobHistory _jobHistory;

		[SetUp]
		public override void SetUp()
		{
			_jobHistory = new JobHistory();
			_jobService = new Mock<IJobService>();
			_jobHistoryService = new Mock<IJobHistoryService>();
			_jobServiceDataProvider = new Mock<IJobServiceDataProvider>();
			_agent = new Mock<IRemovableAgent>();
			_jobHistoryInstanceGuid = Guid.NewGuid();
			_jobId = 123;
			_helper = new Mock<IHelper>();
			_helper.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<JobStopManager>()).Returns(Mock.Of<IAPILog>());
			_instance = PrepareSut(false, new CancellationTokenSource(), new CancellationTokenSource());
		}

		private JobStopManager PrepareSut(bool supportsDrainStop, CancellationTokenSource stopToken, CancellationTokenSource drainStopToken)
		{
			return new JobStopManager(_jobService.Object, _jobHistoryService.Object, _jobServiceDataProvider.Object,
				_helper.Object, _jobHistoryInstanceGuid, _jobId, _agent.Object, supportsDrainStop, stopToken, drainStopToken);
		}

		private static ChoiceRef[] JobHistoryStatuses = new[]
		{
			JobStatusChoices.JobHistoryPending,
			JobStatusChoices.JobHistoryProcessing
		};

		[Test]
		public void IsStopRequested_UnableToFindTheJob()
		{
			// act
			_instance.Execute();
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[TestCase(StopState.None)]
		[TestCase(StopState.Unstoppable)]
		public void IsStopRequested_NoStoppingJob(StopState stopState)
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(stopState);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);

			// act
			_instance.Execute();
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[TestCaseSource(nameof(JobHistoryStatuses))]
		public void IsStopRequested_StoppingJob(ChoiceRef status)
		{
			// arrange
			_jobHistory.JobStatus = status;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);

			// act
			_instance.Execute();
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			_jobHistoryService.Verify(x => x.UpdateRdoWithoutDocuments(It.Is<JobHistory>(history => history.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))));
			Assert.IsTrue(isStopRequested);
		}

		[Test]
		public void IsStopRequested_JobServiceFailsToGetTheJob()
		{
			// arrange
			_jobService.Setup(x => x.GetJob(_jobId)).Throws(new Exception("something"));

			// act
			_instance.Execute();
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[Test]
		public void IsStopRequested_JobHistoryServiceFailsToGetTheJob()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState (StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Throws(new Exception("something"));

			// act
			_instance.Execute();
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[Test]
		public void ThrowIfStopRequested_ThrowExceptionWhenStop()
		{
			// arrange
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			_instance.Execute();

			// act & assert
			Assert.Throws<OperationCanceledException>(() =>	_instance.ThrowIfStopRequested());
		}

		[Test]
		public void StopRequestedEvent_RaisesWhenStop()
		{
			// arrange
			bool eventTriggered = false;
			_instance.StopRequestedEvent += (sender, args) => eventTriggered = true;
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			_instance.Execute();

			// act & assert
			Assert.True(eventTriggered);
		}

		[Test]
		public void ThrowIfStopRequested_DoNotThrowExceptionWhenRunning()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			_instance.Execute();

			// act & assert
			Assert.DoesNotThrow(() => _instance.ThrowIfStopRequested());
		}

		[Test]
		public void StopRequestedEvent_DoesntRaiseWhenRunning()
		{
			// arrange
			bool eventTriggered = false;
			_instance.StopRequestedEvent += (sender, args) => eventTriggered = true;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			_instance.Execute();

			// act & assert
			Assert.False(eventTriggered);
		}

		[Test]
		public void Dispose_CorrectDisposePattern()
		{
			// arrange

			// act & assert
			Assert.DoesNotThrow(() => _instance.Dispose());
			Assert.DoesNotThrow(() => _instance.Dispose());
		}
	}
}