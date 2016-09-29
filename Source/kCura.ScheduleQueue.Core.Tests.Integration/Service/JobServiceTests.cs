﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests.Integration.Service
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
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
			//IntegrationPoint.Tests.Core.Agent.DisableAllAgents();
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
		[Description("When we update the stop state, there is a possibility that the job is already removed from the queue. This scenario will occur when the job is finished before we get to update the job.")]
		public void UpdateStopState_JobDoesNotExist()
		{
			Assert.Throws<InvalidOperationException>(() => _instance.UpdateStopState( new List<long>() {  987654321 }, StopState.Stopping));
		}


		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
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

		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
		public void UpdateStopState_GoldFlow(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(new List<long>() {  job.JobId } , state);

			// assert
			AssertJobStopState(job, state);
		}

		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
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

		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
		public void UpdateStopState_DuplicateJobIds(StopState state)
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);

			// act
			_instance.UpdateStopState(new List<long>() { job.JobId, job.JobId }, state);

			// assert
			AssertJobStopState(job, state);
		}

		[Test]
		[Description("This case will occur when a user click on stop right before the agent set the unstoppable flag.")]
		public void UpdateStopState_SetUnstoppableAfterStopping()
		{
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			List<long> ids = new List<long>() { job.JobId };
			_instance.UpdateStopState(ids, StopState.Stopping);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(ids, StopState.Unstoppable));
		}

		[Test]
		public void UpdateStopState_DoNotAllowStopOnAnUnstoppableJob()
		{
			// arrange
			Job job = _instance.CreateJob(999999, 99999999, TaskType.None.ToString(), DateTime.MaxValue, String.Empty, 9, null, null);
			List<long> ids = new List<long>() { job.JobId };
			_instance.UpdateStopState(ids, StopState.Unstoppable);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => _instance.UpdateStopState(ids, StopState.Stopping));
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

		[TestCase(StopState.None)]
		[TestCase(StopState.Stopping)]
		[TestCase(StopState.Unstoppable)]
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