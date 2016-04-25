using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class AgentJobManagerTest
	{
		private AgentJobManager _manager;
		private IEddsServiceContext _context;
		private IJobService _jobService;
		private ISerializer _serializer;
		private JobTracker _jobTracker;
		private JobResoureTracker _jobResource;
		private IWorkspaceDBContext _workspaceDbContext;
		private int _workspaceId;
		private int _integrationPointId;
		private int _userId;
		private TaskType _task;

		[SetUp]
		public void Setup()
		{
			_workspaceId = 123;
			_integrationPointId = 456;
			_userId = 798;
			_task = TaskType.ExportService;
			_context = Substitute.For<IEddsServiceContext>();
			_context.UserID = 55555;

			_jobService = Substitute.For<IJobService>();
			_serializer = NSubstitute.Substitute.For<ISerializer>();
			_workspaceDbContext = NSubstitute.Substitute.For<IWorkspaceDBContext>();
			_jobResource = new JobResoureTracker(_workspaceDbContext);
			_jobTracker = new JobTracker(_jobResource);
			_manager = new AgentJobManager(_context, _jobService, _serializer, _jobTracker);
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

		private Job GetJob(long jobID, long? rootJobID)
		{
			return JobHelper.GetJob(jobID, rootJobID, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, "",
				0, new DateTime(), 1, null, null);
		}
	}
}
