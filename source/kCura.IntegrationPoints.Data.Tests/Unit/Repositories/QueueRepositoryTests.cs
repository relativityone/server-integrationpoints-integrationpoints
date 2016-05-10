using System;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit.Repositories
{
	public class QueueRepositoryTests
	{
		private IQueueRepository _instance;
		private IHelper _helper;
		private IDBContext _dbContext;
		private int _workspaceId = 12345;
		private int _integrationPointId = 98765;
		private DateTime _runTime = new DateTime(2016, 05, 10);
		private long _jobId = 4141;

		[SetUp]
		public void Setup()
		{
			_dbContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(-1).Returns(_dbContext);

			_instance = new QueueRepository(_helper);

			_helper.Received().GetDBContext(-1);
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_NonZero()
		{
			//Arrange
			string queuedOrRunningSql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			string scheduledRunningSql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is not null
										AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);

			_dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql).Returns(5);
			_dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql).Returns(2);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 7);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(scheduledRunningSql);
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_Zero()
		{
			//Arrange
			string queuedOrRunningSql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			string scheduledRunningSql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is not null
										AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);

			_dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql).Returns(0);
			_dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(scheduledRunningSql);
		}

		[Test]
		public void GetNumberOfJobsExecuting_NonZero()
		{
			//Arrange
			string dateTimeValue = _runTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null
										AND [NextRunTime] <= '{2}' AND [JobID] != {3}", _workspaceId, _integrationPointId, dateTimeValue, _jobId);


			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(1);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

			//Assert
			Assert.IsTrue(numJobs == 1);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void GetNumberOfJobsExecuting_Zero()
		{
			//Arrange
			string dateTimeValue = _runTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null
										AND [NextRunTime] <= '{2}' AND [JobID] != {3}", _workspaceId, _integrationPointId, dateTimeValue, _jobId);

			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}
	}
}
