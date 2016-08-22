using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using System;

namespace kCura.IntegrationPoints.Core.Contracts.Tests.Unit.Agent
{
	using System.Data;

	[TestFixture]
	public class TaskJobSubmitterTests
	{
		private TaskJobSubmitter _instance;
		private IJobManager _jobManager;
		private Job _testJob;
		private Guid _testGuid;
		private TaskType _task;

		[SetUp]
		public void TestSetup()
		{
			_jobManager = Substitute.For<IJobManager>();
			_testJob = JobExtensions.CreateJob();
			_testGuid = Guid.NewGuid();
			_task = TaskType.None;
			_instance = new TaskJobSubmitter(_jobManager, _testJob, _task, _testGuid);
		}

		[Test]
		public void TestJobSubmitter()
		{
			//Arrange
			string sorawit = "sorawit";

			//Act
			_instance.SubmitJob(sorawit);

			//Assert
			_jobManager.Received(1).CreateJobWithTracker(
				_testJob,
				Arg.Is<TaskParameters>(x => x.BatchInstance == _testGuid && (string)x.BatchParameters == sorawit),
				_task,
				_testGuid.ToString());
		}
	}
}