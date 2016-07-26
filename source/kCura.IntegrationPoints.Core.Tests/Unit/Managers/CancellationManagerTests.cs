using System;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class CancellationManagerTests
	{
		private JobStopManager _instance;
		private IJobService _jobService;
		private IJobHistoryService _jobHistoryService;
		private Guid _guid;
		private int _jobId;

		[SetUp]
		public void Setup()
		{
			_jobService = NSubstitute.Substitute.For<IJobService>();
			_jobHistoryService = NSubstitute.Substitute.For<IJobHistoryService>();
			_guid = Guid.NewGuid();
			_jobId = 123;
			_instance = new JobStopManager(_jobService, _jobHistoryService, _guid, _jobId);
		}

		[Test]
		public void IsStoppingRequested_UnableToFindTheJob()
		{
			// act
			_instance.Callback.Invoke(null);
			bool isCancled = _instance.IsStoppingRequested();

			// assert
			Assert.IsFalse(isCancled);
		}

		[TestCase(StopState.None)]
		[TestCase(StopState.Unstoppable)]
		public void IsStoppingRequested_NoStoppingJob(StopState stopState)
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.UpdateStopState(stopState);
			_jobService.GetJob(_jobId).Returns(job);

			// act
			_instance.Callback.Invoke(null);
			bool isCancled = _instance.IsStoppingRequested();

			// assert
			Assert.IsFalse(isCancled);
		}

		[Test]
		public void IsStoppingRequested_StoppingJob()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.UpdateStopState(StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);

			// act
			_instance.Callback.Invoke(null);
			bool isCancled = _instance.IsStoppingRequested();

			// assert
			Assert.IsTrue(isCancled);
		}

		[Test]
		public void IsStoppingRequested_JobServiceFailsToGetTheJob()
		{
			// arrange
			_jobService.GetJob(_jobId).Throws(new Exception("something"));

			// act
			_instance.Callback.Invoke(null);
			bool isCancled = _instance.IsStoppingRequested();

			// assert
			Assert.IsFalse(isCancled);
		}

		[Test]
		public void IsStoppingRequested_JobHistoryServiceFailsToGetTheJob()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.UpdateStopState(StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Throws(new Exception("something"));

			// act
			_instance.Callback.Invoke(null);
			bool isCancled = _instance.IsStoppingRequested();

			// assert
			Assert.IsFalse(isCancled);
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