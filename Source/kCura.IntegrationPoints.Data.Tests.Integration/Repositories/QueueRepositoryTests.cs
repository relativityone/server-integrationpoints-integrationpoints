using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class QueueRepositoryTests : RelativityProviderTemplate
	{
		private readonly int _RipObjectArtifactId = 666;
		private ITestHelper _helper;
		private IQueueRepository _queueRepo;
		private IJobService _jobService;

		public QueueRepositoryTests() : base("QueueRepositoryTests", null)
		{
		}

		public override void TestSetup()
		{
			_jobService = Container.Resolve<IJobService>();
			_helper = new TestHelper();
			_queueRepo = new QueueRepository(_helper);
			ControlIntegrationPointAgents(false);
		}

		#region GetNumberOfJobsExecutingOrInQueue

		[Test]
		public void OnePendingJobInTheQueue_ExpectOneCount()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId, "Some Random Job",
					DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void OnePendingJobInTheQueue_DontIncludeOtherRipJobs()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId, "lol",
					DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, 667);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void OnePendingJobInTheQueue_DontIncludeJobsFromOtherWorkspaces()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId, "lol",
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

		[Test]
		public void MultiplePendingJobsInTheQueue_ExpectCorrectCount()
		{
			const int length = 1500;
			// arrange
			IList<Job> jobs = new List<Job>();
			try
			{
				for (int index = 0; index < length; index++)
				{
					Job job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
					jobs.Add(job);
				}

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

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

		[Test]
		public void OneScheduledJobInTheQueue_ScheduledJobGetExcluded()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.Today.AddDays(200),
					DayOfMonth = 9,
					EndDate = DateTime.Today.AddDays(250)
				}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void OneScheduledJobAndOnePendingJob_ScheduledJobGetExcluded()
		{
			Job schedualedJob = null;
			Job job = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.Today.AddDays(200),
					DayOfMonth = 9,
					EndDate = DateTime.Today.AddDays(250)
				}, String.Empty, 9, null, null);

				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
				RemoveJobFromTheQueue(job);
			}
		}

		[Test, Timeout(300000)]
		[Description("This test takes sometime to process. It requires the IP agent to be running.")]
		[TestInQuarantine]
		public void OneExecutedScheduledJobInTheQueue_ExpectCountZero()
		{
			ControlIntegrationPointAgents(true);
			// arrange
			IntegrationPointModel model = new IntegrationPointModel()
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = "OneExecutingScheduledJobInTheQueue",
				DestinationProvider = DestinationProvider.ArtifactId,
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
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			// act
			var result = CreateOrUpdateIntegrationPoint(model);
			while (result.LastRun.HasValue == false)
			{
				Thread.Sleep(200);
				result = RefreshIntegrationModel(result);
			}

			int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

			// assert
			Assert.AreEqual(0, count);
		}

		[Test]
		public void OneExcecutingScheduledJobInTheQueue_ExpectOne()
		{
			const int agentId = 123456;
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		[Test]
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
				scheduledJob1 = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(10),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, scheduledJob1.JobId);

				scheduledJob2 = _jobService.CreateJob(SourceWorkspaceArtifactId, anotherIntegrationPoint,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(10),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId2, scheduledJob2.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(scheduledJob1);
				RemoveJobFromTheQueue(scheduledJob2);
			}
		}

		[Test]
		public void TryingToCreateTheSameJobTwice_ExpectAnError()
		{
			const int agentId = 123456;
			Job schedualedJob1 = null;
			try
			{
				// arrange
				schedualedJob1 = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
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
				Assert.Throws<ExecuteSQLStatementFailedException>(() => _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
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

		[Test]
		public void OneExecutingJob_ExpectCount()
		{
			Job job = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				_jobService.GetNextQueueJob(new int[] { SourceWorkspaceArtifactId }, _jobService.AgentTypeInformation.AgentTypeID);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void MultipleRegularJob_ExpectCount()
		{
			Job job = null;
			Job job2 = null;
			try
			{
				// arrange
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);

				job2 = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(2, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
				RemoveJobFromTheQueue(job2);
			}
		}

		[Test]
		public void OneExecutingScheduledJobAndOneRegularJob_ExpectCountBoth()
		{
			const int agentId = 789456;
			Job schedualedJob = null;
			Job job = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
				"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
				{
					StartDate = DateTime.UtcNow.AddDays(-1),
					LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
					Interval = ScheduleInterval.Daily,
					Reoccur = 2,
				}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
						"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu",
						DateTime.MaxValue, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

				// assert
				Assert.AreEqual(2, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_NoJobInTheQueue()
		{
			// act
			int count = _queueRepo.GetNumberOfJobsExecutingOrInQueue(SourceWorkspaceArtifactId, _RipObjectArtifactId);

			// assert
			Assert.AreEqual(0, count);
		}

		#endregion GetNumberOfJobsExecutingOrInQueue

		#region GetNumberOfJobsExecuting

		[Test]
		public void GetNumberOfJobsExecuting_NoJobInTheQueue()
		{
			// act
			int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.Now);

			// assert
			Assert.AreEqual(0, count);
		}

		[Test]
		public void GetNumberOfJobsExecuting_OnePendingJobInTheQueue()
		{
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddYears(1), String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.Now);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void GetNumberOfJobsExecuting_OneExecutingJobInQueue_TryToQueryOnAnotherJob()
		{
			const int agentId = 123456;
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddMinutes(-50), String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void GetNumberOfJobsExecuting_OneExecutingJobInQueue_TheJobGetExcluded()
		{
			const int agentId = 123456;
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Gerron_#snitch",
					DateTime.UtcNow.AddMinutes(-50), String.Empty, 9, null, null);
				AssignJobToAgent(agentId, job.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void GetNumberOfJobsExecuting_ExpiredScheduledJobInQueue_TheJobGetExcluded()
		{
			// arrange
			Job job = null;
			try
			{
				job = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Gerron_#snitch",
					new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-200),
						DayOfMonth = 9,
						EndDate = DateTime.UtcNow.AddDays(-150)
					}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(job);
			}
		}

		[Test]
		public void GetNumberOfJobsExecuting_ActiveScheduledJobInQueue()
		{
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-1),
						LocalTimeOfDay = DateTime.UtcNow.AddMinutes(1).TimeOfDay,
						Interval = ScheduleInterval.Daily,
						Reoccur = 2,
					}, String.Empty, 9, null, null);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.UtcNow);

				// assert
				Assert.AreEqual(0, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		[Test]
		public void GetNumberOfJobsExecuting_ActiveAndExecutingScheduledJobInQueue()
		{
			const int agentId = 789456;
			Job schedualedJob = null;
			try
			{
				// arrange
				schedualedJob = _jobService.CreateJob(SourceWorkspaceArtifactId, _RipObjectArtifactId,
					"Kwuuuuuuuuuuuuuuuuuuuuuuuuuuu", new PeriodicScheduleRule()
					{
						StartDate = DateTime.UtcNow.AddDays(-1),
						LocalTimeOfDay = DateTime.UtcNow.TimeOfDay,
						Interval = ScheduleInterval.Daily,
						Reoccur = 2,
					}, String.Empty, 9, null, null);
				AssignJobToAgent(agentId, schedualedJob.JobId);

				// act
				int count = _queueRepo.GetNumberOfJobsExecuting(SourceWorkspaceArtifactId, _RipObjectArtifactId, 1, DateTime.UtcNow.AddDays(1));

				// assert
				Assert.AreEqual(1, count);
			}
			finally
			{
				RemoveJobFromTheQueue(schedualedJob);
			}
		}

		#endregion GetNumberOfJobsExecuting

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