using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Management.Tests.Tasks.Helpers
{
	[TestFixture]
	public class StuckJobsTests : TestBase
	{
		private const int _WORKSPACE_ID_A = 482698;
		private const int _WORKSPACE_ID_B = 903924;

		private StuckJobs _instance;
		private IJobService _jobService;
		private IRunningJobService _runningJobService;
		private IRSAPIServiceFactory _rsapiServiceFactory;

		public override void SetUp()
		{
			_jobService = Substitute.For<IJobService>();
			_runningJobService = Substitute.For<IRunningJobService>();
			_rsapiServiceFactory = Substitute.For<IRSAPIServiceFactory>();

			_instance = new StuckJobs(_jobService, _runningJobService, _rsapiServiceFactory);
		}

		[Test]
		public void ItShouldFindStuckJobs()
		{
			var now = DateTime.UtcNow;

			var batchInstance2 = "EF1E654E-0164-46C0-9A76-1DFE32C557A9";
			var batchInstance3 = "40B02845-FD6A-4A60-A0E2-7F53F4B89CA8";

			var taskParameter2 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance2)
			};
			var taskParameter3 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance3)
			};

			var job1 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_A, 1, row =>
			{
				row["LockedByAgentID"] = 671;
				return new Job(row);
			});
			var job2 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_B, 2, row =>
			{
				row["LockedByAgentID"] = 671;
				return new Job(row);
			});
			job2.JobDetails = JsonConvert.SerializeObject(taskParameter2);
			var job3 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_B, 3, row =>
			{
				row["LockedByAgentID"] = 671;
				return new Job(row);
			});
			job3.JobDetails = JsonConvert.SerializeObject(taskParameter3);
			var job4 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_B, 4, row => 
			{
				row["LockedByAgentID"] = DBNull.Value;
				return new Job(row);
			});

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

			_runningJobService.GetRunningJobs(_WORKSPACE_ID_A).Returns(new List<RDO>());
			_runningJobService.GetRunningJobs(_WORKSPACE_ID_B).Returns(new List<RDO> {rdo2, rdo3});

			_rsapiServiceFactory.Create(_WORKSPACE_ID_B).JobHistoryLibrary.Read(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == rdo2.ArtifactID))
				.Returns(new List<JobHistory> {new JobHistory {BatchInstance = batchInstance3} });

			// ACT
			var stuckJobs = _instance.FindStuckJobs(new List<int> {_WORKSPACE_ID_A, _WORKSPACE_ID_B});

			// ASSERT
			Assert.That(stuckJobs.ContainsKey(_WORKSPACE_ID_B));
			Assert.That(stuckJobs.Keys.Count, Is.EqualTo(1));
			Assert.That(stuckJobs[_WORKSPACE_ID_B].Count, Is.EqualTo(1));
			Assert.That(string.Equals(stuckJobs[_WORKSPACE_ID_B][0].BatchInstance, batchInstance3, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}