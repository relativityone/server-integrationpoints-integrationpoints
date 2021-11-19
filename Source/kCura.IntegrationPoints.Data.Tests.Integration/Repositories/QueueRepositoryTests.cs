using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Testing.Identification;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class QueueRepositoryTests : RelativityProviderTemplate
	{
		private ITestHelper _helper;
		private IQueueRepository _queueRepo;
		private IJobService _jobService;

		private readonly int _RipObjectArtifactId = 666;

		public QueueRepositoryTests() : base(
			sourceWorkspaceName: "QueueRepositoryTests",
			targetWorkspaceName: null)
		{
		}

		public override void TestSetup()
		{
			_jobService = Container.Resolve<IJobService>();
			_helper = new TestHelper();
			_queueRepo = new QueueRepository(_helper);

			ClearScheduleQueue();
		}

		[IdentifiedTest("c575a5d3-f997-4be5-8568-84b17c277588")]
		public void OnePendingJobInTheQueue_ExpectOneCount()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId, "Some Random Job",
					DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("485aca91-8dba-4392-bab8-e9acf0c05ac1")]
		public void OnePendingJobInTheQueue_DontIncludeOtherRipJobs()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId, "lol",
					DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, 667);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("4df832eb-0738-4e22-ae97-ed2aac8c2fc0")]
		public void OnePendingJobInTheQueue_DontIncludeJobsFromOtherWorkspaces()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId, "lol",
					DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(78945, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("092e75e0-0fe8-416a-b0f3-cd3bad5c9ad7")]
		public void MultiplePendingJobsInTheQueue_ExpectCorrectCount()
		{
			const int length = 1500;
			// arrange
			IList<Job> jobs = new List<Job>();
			try
			{
				for (int index = 0; index < length; index++)
				{
					Job job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
					jobs.Add(job);
				}

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(length, count);
			}
			finally
			{
				for (int index = 0; index < jobs.Count; index++)
				{
					RemoveJobFromTheQueue(jobs[index]);
				}
			}
		}

		[IdentifiedTest("14587f2a-9812-4618-9728-826375bf4598")]
		public void OneScheduledJobInTheQueue_ScheduledJobGetExcluded()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.Today.AddDays(200),
					DayOfMonth = 9,
					EndDate = DateTime.Today.AddDays(250)
				}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("05fa55a2-9f10-4909-a462-db5865db4a5c")]
		public void OneScheduledJobAndOnePendingJob_ScheduledJobGetExcluded()
		{
			Job schedualedJob = null;
			Job job = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.Today.AddDays(200),
					DayOfMonth = 9,
					EndDate = DateTime.Today.AddDays(250)
				}, String.Empty, 9, null, null);

				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("c5eca681-6859-441e-84b0-634c41570f12"), Timeout(300000)]
		[Description("This test takes sometime to process. It requires the IP agent to be running.")]
		[NotWorkingOnTrident]
		public void OneExecutedScheduledJobInTheQueue_ExpectCountZero()
		{
			// arrange
			var model = new IntegrationPointModel
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = "OneExecutingScheduledJobInTheQueue",
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				Map = CreateDefaultFieldMap(),
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					EndDate = DateTime.UtcNow.AddYears(1000).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					Reoccur = 2,
					StartDate = DateTime.UtcNow.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = DateTime.UtcNow.AddMinutes(1).TimeOfDay.ToString(),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				SelectedOverwrite = "Append Only",
				Type = Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
					.ArtifactId
			};

			// act
			IntegrationPointModel result = CreateOrUpdateIntegrationPoint(model);
			while (!result.LastRun.HasValue)
			{
				Thread.Sleep(200);
				result = RefreshIntegrationModel(result);
			}

			int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID,
				_RipObjectArtifactId);

			// assert
			Assert.AreEqual(0, count);
		}

		[IdentifiedTest("046454d4-1d10-45fa-9c1a-148d259192c5")]
		public void OneExcecutingScheduledJobInTheQueue_ExpectOne()
		{
			const int agentId = 123456;
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		[IdentifiedTest("9dc7c0ca-3408-43ed-ba23-bbccf4ec2146")]
		public void TwoScheduledJobsExecutingAtTheSameTimeOnDifferentAgents()
		{
			const int agentId = 123456;
			const int agentId2 = 12345226;
			const int anotherIntegrationPoint = 88885;
			Job scheduledJob1 = null;
			Job scheduledJob2 = null;
			try
			{
				// arrange
				scheduledJob1 = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(10),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, scheduledJob1.JobId);

				scheduledJob2 = _jobService.CreateJob(SourceWorkspaceArtifactID, anotherIntegrationPoint,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(10),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId2, scheduledJob2.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(scheduledJob1);
				RemoveJobFromTheQueue(scheduledJob2);
			}
		}

		[IdentifiedTest("b1d02711-f078-4ff7-a652-f0d099b4c3ff")]
		public void TryingToCreateTheSameJobTwice_ExpectAnError()
		{
			const int agentId = 123456;
			Job schedualedJob1 = null;
			try
			{
				// arrange
				schedualedJob1 = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob1.JobId);
				int initialCount = CountJobsInTheQueue();

				// act
				Assert.Throws<ExecuteSQLStatementFailedException>(() => _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null), "Error: Job is currently being executed by Agent and is locked for updates.");

				int eventualCount = CountJobsInTheQueue();
				Assert.AreEqual(initialCount, eventualCount);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob1);
			}
		}

		[IdentifiedTest("6b4b7ef2-0416-4e23-8c64-b625f075c706")]
		public void OneExecutingJob_ExpectCount()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				_jobService.GetNextQueueJob(new int[] { SourceWorkspaceArtifactID }, _jobService.AgentTypeInformation.AgentTypeID);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("1a881168-4924-48e6-b4b8-7b00e8d35601")]
		public void MultipleRegularJob_ExpectCount()
		{
			Job job = null;
			Job job2 = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);

				job2 = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(2, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
				RemoveJobFromTheQueue(job2);
			}
		}

		[IdentifiedTest("866217ab-bb29-4a3e-b068-5e2a05e26177")]
		[Ignore("")]
		public void OneExecutingScheduledJobAndOneRegularJob_ExpectCountBoth()
		{
			const int agentId = 789456;
			Job schedualedJob = null;
			Job job = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(2, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("2277df26-7eb1-4ac4-86a9-de36ab381829")]
		public void GetNumberOfJobsExecutingOrInQueue_NoJobInTheQueue()
		{
			// act
			int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactID, _RipObjectArtifactId);

			// assert
			Assert.AreEqual(0, count);
		}
		
		[IdentifiedTest("49c4552b-fd4f-4161-a311-04a7f6ae286a")]
		public void GetNumberOfJobsExecuting_NoJobInTheQueue()
		{
			// act
			int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.Now);

			// assert
			Assert.AreEqual(0, count);
		}

		[IdentifiedTest("9bde7c7e-7850-4508-b116-658357e36af2")]
		public void GetNumberOfJobsExecuting_OnePendingJobInTheQueue()
		{
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddYears(1), String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.Now);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("379a08ac-52e3-43ff-9659-6032b6ab1fe8")]
		public void GetNumberOfJobsExecuting_OneExecutingJobInQueue_TryToQueryOnAnotherJob()
		{
			const int agentId = 123456;
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddMinutes(-50), String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("6dd2505c-33ff-49ca-8f08-db2a4ec354ca")]
		[Ignore("")]
		public void GetNumberOfJobsExecuting_OneExecutingJobInQueue_TheJobGetExcluded()
		{
			const int agentId = 123456;
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddMinutes(-50), String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("e3cb1fab-57d8-4b51-b91e-b0cb68a4125c")]
		public void GetNumberOfJobsExecuting_ExpiredScheduledJobInQueue_TheJobGetExcluded()
		{
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Gerron_#snitch",
					new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-200),
						DayOfMonth = 9,
						EndDate = DateTime.UtcNow.AddDays(-150)
					}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[IdentifiedTest("d5db3182-a04b-46cd-bc0b-c5b2907e8acb")]
		public void GetNumberOfJobsExecuting_ActiveScheduledJobInQueue()
		{
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-1),
						LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
						Interval = ScheduleInterval.Daily,
						Reoccur = 2,
					}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		[IdentifiedTest("0fa7650c-bef7-40c9-b93e-fb5525988b1b")]
		public void GetNumberOfJobsExecuting_ActiveAndExecutingScheduledJobInQueue()
		{
			const int agentId = 789456;
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactID, _RipObjectArtifactId,
					"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-1),
						LocalTimeOfDay = DateTime.UtcNow.TimeOfDay,
						Interval = ScheduleInterval.Daily,
						Reoccur = 2,
					}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactID, _RipObjectArtifactId, 1, DateTime.UtcNow.AddDays(1));

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		private void ClearScheduleQueue()
		{
			foreach (Job job in _jobService.GetAllScheduledJobs())
			{
				RemoveJobFromTheQueue(job);
			}
		}

		private void RemoveJobFromTheQueue(Job job)
		{
			if (job != null)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}

		private int CountJobsInTheQueue()
		{
			string query = $" SELECT COUNT(*) FROM [{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}]";
			int count = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(query);
			return count;
		}
	}
}