using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Management.Tests.Tasks.Helpers
{
	[TestFixture]
	public class StuckJobsTests : TestBase
	{
		private IJobRepository _runningJobRepository;
		private IJobService _jobService;
		private StuckJobs _instance;

		private const int _WORKSPACE_ID_A = 482698;
		private const int _WORKSPACE_ID_B = 903924;

		public override void SetUp()
		{
			_jobService = Substitute.For<IJobService>();
			_runningJobRepository = Substitute.For<IJobRepository>();
			_instance = new StuckJobs(_jobService, _runningJobRepository);
		}

		[Test]
		public void ItShouldFindStuckJobs()
		{
			// ARRANGE
			DateTime now = DateTime.UtcNow;

			const int agentId = 671;
			const string batchInstance2 = "EF1E654E-0164-46C0-9A76-1DFE32C557A9";
			const string batchInstance3 = "40B02845-FD6A-4A60-A0E2-7F53F4B89CA8";

			var taskParameter2 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance2)
			};

			var taskParameter3 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance3)
			};
			
			Job job1 = new JobBuilder().WithJobId(1).WithWorkspaceId(_WORKSPACE_ID_A).WithLockedByAgentId(agentId).Build();
			Job job2 = new JobBuilder().WithJobId(2).WithWorkspaceId(_WORKSPACE_ID_B).WithLockedByAgentId(agentId).WithJobDetails(taskParameter2).Build();
			Job job3 = new JobBuilder().WithJobId(3).WithWorkspaceId(_WORKSPACE_ID_B).WithLockedByAgentId(agentId).WithJobDetails(taskParameter3).Build();
			Job job4 = new JobBuilder().WithJobId(4).WithWorkspaceId(_WORKSPACE_ID_B).WithLockedByAgentId(DBNull.Value).Build();

			var rdo2 = new RDO(593)
			{
				Fields = new List<FieldValue> {new FieldValue(new Guid(JobHistoryFieldGuids.BatchInstance), batchInstance2)},
				SystemLastModifiedOn = now.AddHours(-2)
			};
			var rdo3 = new RDO
			{
				Fields = new List<FieldValue> {new FieldValue(new Guid(JobHistoryFieldGuids.BatchInstance), batchInstance3)},
				SystemLastModifiedOn = now.AddMinutes(-45)
			};

			_jobService.GetAllScheduledJobs().Returns(new List<Job> {job1, job2, job3, job4});

			_runningJobRepository.GetRunningJobs(_WORKSPACE_ID_A).Returns(new List<RDO>());
			_runningJobRepository.GetRunningJobs(_WORKSPACE_ID_B).Returns(new List<RDO> {rdo2, rdo3});
			_runningJobRepository.GetStuckJobs(Arg.Any<IList<int>>(), _WORKSPACE_ID_B).Returns(new List<JobHistory> {new JobHistory {BatchInstance = batchInstance3}});

			// ACT
			IDictionary<int, IList<JobHistory>> stuckJobs = _instance.FindStuckJobs(new List<int> {_WORKSPACE_ID_A, _WORKSPACE_ID_B});

			// ASSERT
			Assert.That(stuckJobs.ContainsKey(_WORKSPACE_ID_B));
			Assert.That(stuckJobs.Keys.Count, Is.EqualTo(1));
			Assert.That(stuckJobs[_WORKSPACE_ID_B].Count, Is.EqualTo(1));
			Assert.That(string.Equals(stuckJobs[_WORKSPACE_ID_B][0].BatchInstance, batchInstance3, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}