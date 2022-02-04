using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture, Category("Unit")]
	public class AgentJobManagerTests : TestBase
	{
		private AgentJobManager _manager;
		private IEddsServiceContext _context;
		private IJobService _jobService;
		private IIntegrationPointSerializer _serializer;
		private IJobTrackerQueryManager _jobTrackerQueryManager;
		private IQueueQueryManager _queueQueryManager;
		private IHelper _helper;
		private JobTracker _jobTracker;
		private JobResourceTracker _jobResource;
		private int _workspaceId;
		private int _integrationPointId;
		private int _userId;
		private TaskType _task;

		[SetUp]
		public override void SetUp()
		{
			_workspaceId = 123;
			_integrationPointId = 456;
			_userId = 798;
			_task = TaskType.ExportService;
			_context = Substitute.For<IEddsServiceContext>();
			_context.UserID = 55555;

			_helper = Substitute.For<IHelper>();
			_jobService = Substitute.For<IJobService>();
			_serializer = Substitute.For<IIntegrationPointSerializer>();
			_jobTrackerQueryManager = Substitute.For<IJobTrackerQueryManager>();
			_queueQueryManager = Substitute.For<IQueueQueryManager>();
			_jobResource = new JobResourceTracker(_jobTrackerQueryManager, _queueQueryManager);
			_jobTracker = new JobTracker(_jobResource);
			_manager = new AgentJobManager(_context, _jobService, _helper, _serializer, _jobTracker);
		}

		[Test]
		public void CreateJobOnBehalfOfAUser_GoldFlow()
		{
			object jobDetail = null;

			_manager.CreateJobOnBehalfOfAUser(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId, _userId);

			_serializer.DidNotReceive().Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _userId, null, null);
		}

		[Test]
		public void CreateJobOnBehalfOfAUser_Error()
		{
			object jobDetail = null;

			_jobService.CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _userId, null, null)
				.Throws(new AgentNotFoundException());

			Assert.Throws<Exception>(() => _manager.CreateJobOnBehalfOfAUser(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId, _userId));

			_serializer.DidNotReceive().Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _userId, null, null);
		}

		[Test]
		public void CreateJobOnBehalfOfAUser_GoldFlow_WithSerializeData()
		{
			string jobDetail = "some details";

			_manager.CreateJobOnBehalfOfAUser(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId, _userId);

			_serializer.Received(1).Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), String.Empty, _userId, null, null);
		}

		[Test]
		public void CreateJob_GoldFlow()
		{
			object jobDetail = null;

			_manager.CreateJob(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId);

			_serializer.DidNotReceive().Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _context.UserID, null, null);
		}

		[Test]
		public void CreateJob_Error()
		{
			object jobDetail = null;

			_jobService.CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _context.UserID, null, null)
				.Throws(new AgentNotFoundException());

			Assert.Throws<Exception>(() => _manager.CreateJob(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId));

			_serializer.DidNotReceive().Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), null, _context.UserID, null, null);
		}

		[Test]
		public void CreateJob_GoldFlow_WithSerializeData()
		{
			string jobDetail = "some details";

			_manager.CreateJob(jobDetail, TaskType.ExportService, _workspaceId, _integrationPointId);

			_serializer.Received(1).Serialize(jobDetail);
			_jobService.Received(1).CreateJob(_workspaceId, _integrationPointId, _task.ToString(), Arg.Any<DateTime>(), String.Empty, _context.UserID, null, null);
		}

		[Test]
		public void GetRootJobId_RootJobIDIsNull_ParentJobID()
		{
			//ARRANGE
			Job parentJob = GetJob(222, null);

			//ACT
			long? rootJobID = AgentJobManager.GetRootJobId(parentJob);

			//ASSERT
			Assert.AreEqual(222, rootJobID);
		}

		[Test]
		public void GetRootJobId_RootJobIDIsNotNull_ParentRootJobID()
		{
			//ARRANGE
			Job parentJob = GetJob(222, 101);

			//ACT
			long? rootJobID = AgentJobManager.GetRootJobId(parentJob);

			//ASSERT
			Assert.AreEqual(101, rootJobID);
		}

		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_NoJobs()
		{
			// arrange
			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>());

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsEmpty(batchInstanceToJob);
		}
		
		[Test]
		public void GetJobsByBatchInstanceId_ReturnEmptyDictionary_WhenNullReturned()
        {
			// arrange
			_jobService.GetJobs(_integrationPointId).Returns(null as List<Job>);

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsEmpty(batchInstanceToJob);
		}

		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_SingleJob()
		{
			// arrange
			TaskParameters parameter = new TaskParameters { BatchInstance = Guid.NewGuid() };
			Job job = new JobBuilder()
				.WithWorkspaceId(1)
				.WithRelatedObjectArtifactId(2)
				.WithJobDetails(parameter)
				.Build();
			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameter);

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(parameter.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[parameter.BatchInstance].Contains(job));
		}


		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_MultiJobsOnOneBatchInstanceId()
		{
			// arrange
			TaskParameters parameter = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job = new JobBuilder()
				.WithWorkspaceId(1)
				.WithRelatedObjectArtifactId(2)
				.WithJobDetails(parameter)
				.Build();
			Job job2 = new JobBuilder().WithJob(job).Build();
			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameter);

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(parameter.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[parameter.BatchInstance].SequenceEqual(new List<Job>() { job, job2 }));
		}

		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_MultiJobsOnMultipleBatchInstanceIds()
		{
			// arrange
			var jobOneParameters = new TaskParameters { BatchInstance = Guid.NewGuid() };
			Job job = new JobBuilder()
				.WithWorkspaceId(1)
				.WithRelatedObjectArtifactId(2)
				.WithJobDetails(jobOneParameters)
				.Build();

			var jobTwoParameters = new TaskParameters { BatchInstance = Guid.NewGuid() };
			Job job2 = new JobBuilder().WithJob(job).WithJobDetails(jobTwoParameters).Build();

			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(jobOneParameters);
			_serializer.Deserialize<TaskParameters>(job2.JobDetails).Returns(jobTwoParameters);


			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(jobOneParameters.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[jobOneParameters.BatchInstance].SequenceEqual(new List<Job>() { job }));
			Assert.IsTrue(batchInstanceToJob.ContainsKey(jobTwoParameters.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[jobTwoParameters.BatchInstance].SequenceEqual(new List<Job>() { job2 }));
		}

		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_SerialzationFailsOnOneOfTheJobs()
		{
			// arrange
			var jobOneParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job = new JobBuilder()
				.WithWorkspaceId(1)
				.WithRelatedObjectArtifactId(2)
				.WithJobDetails(jobOneParameters)
				.Build();

			var jobTwoParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job2 = new JobBuilder().WithJob(job).WithJobDetails(jobTwoParameters).Build();

			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(jobOneParameters);
			_serializer.Deserialize<TaskParameters>(job2.JobDetails).Throws(new Exception("blah"));

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetJobsByBatchInstanceId(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(jobOneParameters.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[jobOneParameters.BatchInstance].SequenceEqual(new List<Job>() { job }));
			Assert.IsFalse(batchInstanceToJob.ContainsKey(jobTwoParameters.BatchInstance));
		}

		private Job GetJob(long jobID, long? rootJobID)
		{
			return JobHelper.GetJob(jobID, rootJobID, null, 1, 1, 111, 222, TaskType.SyncEntityManagerWorker, new DateTime(), null, "",
				0, new DateTime(), 1, null, null);
		}


	}
}
