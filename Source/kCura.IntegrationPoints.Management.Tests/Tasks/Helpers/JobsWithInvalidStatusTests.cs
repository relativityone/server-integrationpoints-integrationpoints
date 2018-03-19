using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Tests.Tasks.Helpers
{
	[TestFixture]
	public class JobsWithInvalidStatusTests : TestBase
	{
		private const int _WORKSPACE_ID_A = 482698;
		private const int _WORKSPACE_ID_B = 903924;
		private const string _INVALID_JOB_BATCH_INSTANCE = "1402CD2A-3482-4D94-8004-B1A45F4283C4";

		private JobsWithInvalidStatus _instance;
		private IUnfinishedJobService _unfinishedJobService;
		private IJobService _jobService;

		public override void SetUp()
		{
			_unfinishedJobService = Substitute.For<IUnfinishedJobService>();
			_jobService = Substitute.For<IJobService>();
			IAPILog logger = Substitute.For<IAPILog>();
			_instance = new JobsWithInvalidStatus(_unfinishedJobService, new IntegrationPointSerializer(logger), _jobService);
		}

		[Test]
		public void ItShouldFindInvalidJobs()
		{
			var testData = CreateTestData();

			_jobService.GetAllScheduledJobs().Returns(testData.Item1.Keys);
			foreach (var workspaceId in testData.Item2.Keys)
			{
				_unfinishedJobService.GetUnfinishedJobs(workspaceId).Returns(new List<JobHistory> {testData.Item2[workspaceId]});
			}

			// ACT
			var invalidJobs = _instance.Find(new List<int> {_WORKSPACE_ID_A, _WORKSPACE_ID_B});

			// ASSERT
			Assert.That(invalidJobs.ContainsKey(_WORKSPACE_ID_B));
			Assert.That(invalidJobs.Keys.Count, Is.EqualTo(1));
			Assert.That(invalidJobs[_WORKSPACE_ID_B].Count, Is.EqualTo(1));
			Assert.That(invalidJobs[_WORKSPACE_ID_B][0].BatchInstance == _INVALID_JOB_BATCH_INSTANCE);
		}

		/// <summary>
		///     Two workspaces (A,B) with two jobs each (A1, A2, B1, B2)
		///     Two unfinished jobs - one in first workspace (A2) and one in second workspace (B2)
		///     Three scheduled jobs (A1, A2, B1)
		///     One invalid job (B2)
		/// </summary>
		/// <returns></returns>
		private Tuple<Dictionary<Job, TaskParameters>, Dictionary<int, JobHistory>> CreateTestData()
		{
			var batchInstance1 = "7875DB0C-8CD7-4A47-BBD4-1DB220CC53C3";
			var batchInstance2 = "EF1E654E-0164-46C0-9A76-1DFE32C557A9";
			var batchInstance3 = "40B02845-FD6A-4A60-A0E2-7F53F4B89CA8";


			var taskParameter1 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance1)
			};
			var taskParameter2 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance2)
			};
			var taskParameter3 = new TaskParameters
			{
				BatchInstance = new Guid(batchInstance3)
			};

			var job1 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_A, 1, row => new Job(row));
			job1.JobDetails = JsonConvert.SerializeObject(taskParameter1);
			var job2 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_A, 2, row => new Job(row));
			job2.JobDetails = JsonConvert.SerializeObject(taskParameter2);
			var job3 = JobExtensions.CreateJob<Job>(_WORKSPACE_ID_B, 3, row => new Job(row));
			job3.JobDetails = JsonConvert.SerializeObject(taskParameter3);

			Dictionary<Job, TaskParameters> scheduledJobs = new Dictionary<Job, TaskParameters>
			{
				{job1, taskParameter1},
				{job2, taskParameter2},
				{job3, taskParameter3}
			};

			Dictionary<int, JobHistory> unfinishedJobs = new Dictionary<int, JobHistory>
			{
				{_WORKSPACE_ID_A, new JobHistory {BatchInstance = batchInstance2}},
				{_WORKSPACE_ID_B, new JobHistory {BatchInstance = _INVALID_JOB_BATCH_INSTANCE}}
			};

			return new Tuple<Dictionary<Job, TaskParameters>, Dictionary<int, JobHistory>>(scheduledJobs, unfinishedJobs);
		}
	}
}