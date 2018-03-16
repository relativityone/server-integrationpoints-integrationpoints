using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture]
    public class AgentJobManagerTest : TestBase
    {
        private AgentJobManager _manager;
        private IEddsServiceContext _context;
        private IJobService _jobService;
        private IIntegrationPointSerializer _serializer;
	    private IHelper _helper;
        private JobTracker _jobTracker;
        private JobResourceTracker _jobResource;
        private IWorkspaceDBContext _workspaceDbContext;
        private IRepositoryFactory _repositoryFactory;
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
            _serializer = NSubstitute.Substitute.For<IIntegrationPointSerializer>();
            _workspaceDbContext = NSubstitute.Substitute.For<IWorkspaceDBContext>();
            _repositoryFactory = NSubstitute.Substitute.For<IRepositoryFactory>();
            _jobResource = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
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
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsEmpty(batchInstanceToJob);
		}

		[Test]
	    public void GetScheduledAgentJobMapedByBatchInstance_SingleJob()
	    {
			// arrange
			JSONSerializer serializer = new JSONSerializer();
			TaskParameters parameter = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job = JobExtensions.CreateJob(1, 2, serializer.Serialize(parameter));
			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameter);

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(parameter.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[parameter.BatchInstance].Contains(job));
		}


		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_MultiJobsOnOneBatchInstanceId()
		{
			// arrange
			JSONSerializer serializer = new JSONSerializer();
			TaskParameters parameter = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			string details = serializer.Serialize(parameter);
			Job job = JobExtensions.CreateJob(1, 2, details);
			Job job2 = JobExtensions.CreateJob(1, 2, details);
			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameter);

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(parameter.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[parameter.BatchInstance].SequenceEqual(new List<Job>() { job, job2 }));
		}

		[Test]
		public void GetScheduledAgentJobMapedByBatchInstance_MultiJobsOnMultipleBatchInstanceIds()
		{
			// arrange
			JSONSerializer serializer = new JSONSerializer();

			TaskParameters jobOneParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job = JobExtensions.CreateJob(1, 2, serializer.Serialize(jobOneParameters));

			TaskParameters jobTwoParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job2 = JobExtensions.CreateJob(1, 2, serializer.Serialize(jobTwoParameters));

			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(jobOneParameters);
			_serializer.Deserialize<TaskParameters>(job2.JobDetails).Returns(jobTwoParameters);


			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointId);

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
			JSONSerializer serializer = new JSONSerializer();

			TaskParameters jobOneParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job = JobExtensions.CreateJob(1, 2, serializer.Serialize(jobOneParameters));

			TaskParameters jobTwoParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			Job job2 = JobExtensions.CreateJob(1, 2, serializer.Serialize(jobTwoParameters));

			_jobService.GetJobs(_integrationPointId).Returns(new List<Job>() { job, job2 });
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(jobOneParameters);
			_serializer.Deserialize<TaskParameters>(job2.JobDetails).Throws(new Exception("blah"));

			// act
			IDictionary<Guid, List<Job>> batchInstanceToJob = _manager.GetScheduledAgentJobMapedByBatchInstance(_integrationPointId);

			// assert
			Assert.IsNotNull(batchInstanceToJob);
			Assert.IsTrue(batchInstanceToJob.ContainsKey(jobOneParameters.BatchInstance));
			Assert.IsTrue(batchInstanceToJob[jobOneParameters.BatchInstance].SequenceEqual(new List<Job>() { job }));
			Assert.IsFalse(batchInstanceToJob.ContainsKey(jobTwoParameters.BatchInstance));
		}

		private Job GetJob(long jobID, long? rootJobID)
        {
            return JobHelper.GetJob(jobID, rootJobID, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, "",
                0, new DateTime(), 1, null, null);
        }


    }
}
