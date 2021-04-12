using System;
using System.Collections.Generic;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;

namespace kCura.ScheduleQueue.Core.Tests.Integration.Service
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class JobServiceTests
	{
		private readonly Guid _agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

		private IAgentService _agentService;
		private IHelper _helper;
		private JobService _instance;
		private JobServiceDataProvider _jobServiceDataProvider;

		[SetUp]
		public void SetUp()
		{
			kCura.Data.RowDataGateway.Config.SetConnectionString(SharedVariables.EddsConnectionString);
			
			_helper = Substitute.For<IHelper>();

			var queryManager = new QueryManager(_helper, _agentGuid);

			_agentService = new AgentService(_helper, queryManager, _agentGuid);
			Context baseContext = new Context(SharedVariables.EddsConnectionString);
			IDBContext dBContext = DBContextMockBuilder.Build(baseContext);
			_helper.GetDBContext(-1).Returns(dBContext);

			_jobServiceDataProvider = new JobServiceDataProvider(queryManager);
			_instance = new JobService(_agentService, _jobServiceDataProvider, _helper);
		}

		[TearDown]
		public void TearDown()
		{
			string query = $"Delete From [eddsdbo].[{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}]";
			_helper.GetDBContext(-1).ExecuteNonQuerySQLStatement(query);
		}

		[IdentifiedTestCase("009766e9-c334-4628-9b6a-e681404b7aa8", StopState.None)]
		[IdentifiedTestCase("0fb6d2f7-9779-494b-bef9-045f5ec36f95", StopState.Stopping)]
		[IdentifiedTestCase("dd30600a-568c-4818-9dbd-15c2a76519ad", StopState.Unstoppable)]
		[Description("This scenario will occur when the some sub-jobs finishes before we get to update the job. We do not expect any error as the job should be stopped still.")]
		public void UpdateStopState_SomeJobsDoNotExist(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			Assert.DoesNotThrow(() => _instance.UpdateStopState(new List<long>() { job.JobId, 987654321 }, state));

			// assert
			AssertJobStopState(job, state);
		}

		[SmokeTest]
		[IdentifiedTestCase("588db8b9-fd75-48ba-ab57-81b18dd4cb00", StopState.None)]
		[IdentifiedTestCase("c5f760a1-383c-4d9e-b4c6-0bead45b54d7", StopState.Stopping)]
		[IdentifiedTestCase("a8c31ae3-ae96-4533-b23b-7a49aeebfff5", StopState.Unstoppable)]
		public void UpdateStopState_GoldFlow(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(new List<long>() {  job.JobId } , state);

			// assert
			AssertJobStopState(job, state);
		}

		[IdentifiedTestCase("dfc55e47-ef67-4631-b85f-37e9a078e69d", StopState.None)]
		[IdentifiedTestCase("bd8cb0c2-3002-43d8-ac1b-fabc2a68c587", StopState.Stopping)]
		[IdentifiedTestCase("c89eb355-009b-4214-bcfe-98eed875ea81", StopState.Unstoppable)]
		public void UpdateStopState_MultipleJobIds(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			Job job2 = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			Job job3 = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(new List<long>() { job.JobId, job2.JobId, job3.JobId, job.JobId, 654987 }, state);

			// assert
			AssertJobStopState(job, state);
			AssertJobStopState(job2, state);
			AssertJobStopState(job3, state);
		}

		[IdentifiedTestCase("be695a7f-e5f7-4f7e-b3ba-06c90c016802", StopState.None)]
		[IdentifiedTestCase("4e2cad47-ea14-4c35-a408-acc3b1485368", StopState.Stopping)]
		[IdentifiedTestCase("f6d14e54-9ba1-45a4-8439-b7549ce294ce", StopState.Unstoppable)]
		public void UpdateStopState_DuplicateJobIds(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(new List<long>() { job.JobId, job.JobId }, state);

			// assert
			AssertJobStopState(job, state);
		}

		[IdentifiedTest("8125db7a-a054-4cc3-930b-c7bdcabfe87c")]
		[Description("This case will occur when a user click on stop right before the agent set the unstoppable flag.")]
		public void UpdateStopState_SetUnstoppableAfterStopping()
		{
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			List<long> ids = new List<long>() { job.JobId };
			_instance.UpdateStopState(ids, StopState.Stopping);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(ids, StopState.Unstoppable));
		}

		[IdentifiedTest("cf0f4cae-59c1-4148-89d7-93dd91585084")]
		public void UpdateStopState_DoNotAllowStopOnAnUnstoppableJob()
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			List<long> ids = new List<long>() { job.JobId };
			_instance.UpdateStopState(ids, StopState.Unstoppable);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(ids, StopState.Stopping));
		}

		[IdentifiedTestCase("20242fa5-38cc-48a5-8b2e-b14ce5bfdee8", StopState.None)]
		[IdentifiedTestCase("d711fbea-9b4e-4562-8842-b558ef3a7fab", StopState.Stopping)]
		[IdentifiedTestCase("0f24d035-6bad-42cc-b7a2-23e3dc355702", StopState.Unstoppable)]
		public void CreateJob_ChildJobGetParentStopState(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			_instance.UpdateStopState(new List<long>() { job.JobId }, state);

			// act
			Job childJob = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, job.JobId);

			// assert
			Assert.AreEqual(childJob.StopState, state);
		}

		private void AssertJobStopState(Job job, StopState state)
		{
			Job updatedJob = _instance.GetJob(job.JobId);
			Assert.AreEqual(updatedJob.StopState, state);
		}
	}
}