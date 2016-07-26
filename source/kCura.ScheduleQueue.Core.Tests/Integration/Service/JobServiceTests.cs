using System;
using System.Collections.Generic;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
	[Explicit("TODO : these tests need to run when the rip agents are disabled.")]
	public class JobServiceTests
	{
		private const string _AGENT_TYPEID = "AgentTypeID";
		private const string _AGENT_NAME = "Name";
		private const string _AGENT_FULLNAMESPACE = "Fullnamespace";
		private const string _AGENT_GUID = "Guid";

		private IAgentService _agentService;
		private IHelper _helper;
		private JobService _instance;
		private AgentTypeInformation _agentInfo;

		[SetUp]
		public void SetUp()
		{
			using (DataTable table = new DataTable())
			{
				table.Columns.Add(new DataColumn(_AGENT_TYPEID, typeof(int)));
				table.Columns.Add(new DataColumn(_AGENT_NAME, typeof(String)));
				table.Columns.Add(new DataColumn(_AGENT_FULLNAMESPACE, typeof(String)));
				table.Columns.Add(new DataColumn(_AGENT_GUID, typeof(Guid)));

				DataRow row = table.NewRow();
				row[_AGENT_TYPEID] = 999;
				row[_AGENT_NAME] = "bad agent";
				row[_AGENT_FULLNAMESPACE] = "whatever";
				row[_AGENT_GUID] = "f5d67f54-0e70-4fbd-b59e-25383e057311";

				_agentInfo = new AgentTypeInformation(row);
			}

			_agentService = Substitute.For<IAgentService>();
			_agentService.QueueTable.Returns(GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
			_agentService.AgentTypeInformation.Returns(_agentInfo);
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(-1).Returns(new DBContext(new Context(SharedVariables.EddsConnectionString)));
			_instance = new JobService(_agentService, _helper);
		}

		[TearDown]
		public void TearDown()
		{
			string query = $"Delete From [eddsdbo].[{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}]";
			_helper.GetDBContext(-1).ExecuteNonQuerySQLStatement(query);
		}

		[Test]
		public void CreateJob_NoneStoppingState()
		{
			// act
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// assert
			Assert.AreEqual(job.StopState, StopState.None);
		}

		[Test]
		public void UpdateStopState_JobDoesNotExist()
		{
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(987654321, StopState.Stopping));
		}

		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
		public void UpdateStopState_GoldFlow(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(job.JobId, state);

			// assert
			Job updatedJob = _instance.GetJob(job.JobId);
			Assert.AreEqual(updatedJob.StopState, state);
		}

		[Test]
		[Description("This case will occur when a user click on stop right before the agent set the unstoppable flag.")]
		public void UpdateStopState_SetUnstoppableAfterStopping()
		{
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			_instance.UpdateStopState(job.JobId, StopState.Stopping);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(job.JobId, StopState.Unstoppable));
		}

		[Test]
		public void UpdateStopState_DoNotAllowStopOnAnUnstoppableJob()
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			_instance.UpdateStopState(job.JobId, StopState.Unstoppable);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(job.JobId, StopState.Stopping));
		}

		[Test]
		public void GetJobs_NoJobsEmptyTable()
		{
			// act
			IList<Job> jobs = _instance.GetJobs(-1);

			// assert
			Assert.IsNotNull(jobs);
			Assert.IsEmpty(jobs);
		}

		[Test]
		public void GetJobs_NoJobs()
		{
			// arrange
			int integrationPointArtifactIds = 789654123;
			_instance.CreateJob(999999, integrationPointArtifactIds, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			_instance.CreateJob(999999, integrationPointArtifactIds, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			IList<Job> jobs = _instance.GetJobs(-1);

			// assert
			Assert.IsNotNull(jobs);
			Assert.IsEmpty(jobs);
		}

		[Test]
		public void GetJobs_FoundMatches()
		{
			// arrange
			int integrationPointArtifactIds = 789654123;
			_instance.CreateJob(999999, integrationPointArtifactIds, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			_instance.CreateJob(999999, integrationPointArtifactIds, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			IList<Job> jobs = _instance.GetJobs(integrationPointArtifactIds);

			// assert
			Assert.IsNotNull(jobs);
			Assert.AreEqual(2, jobs.Count);
			// TODO : add more verifications
		}
	}
}