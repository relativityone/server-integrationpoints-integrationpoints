namespace kCura.IntegrationPoints.Core.Contracts.Tests.Unit.Agent
{
	using System;

	using kCura.IntegrationPoints.Core.Contracts.Agent;
	using kCura.ScheduleQueue.Core;

	using NSubstitute;

	using NUnit.Framework;

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
			_testJob = null;
			_testGuid = Guid.NewGuid();
			_task = TaskType.None;
			_instance = new TaskJobSubmitter(_jobManager, _testJob, _task, _testGuid);
		}

		[Test]
		public void TestJobSubmitter()
		{
			string sorawit = "sorawit";
			_instance.SubmitJob(sorawit);
			_jobManager.Received(1).CreateJobWithTracker(
				_testJob, 
				Arg.Is<TaskParameters>(x => x.BatchInstance == _testGuid && (string)x.BatchParameters == sorawit), 
				_task,
				_testGuid.ToString());
		}
	}
}