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
		private IDBContext _dbContext;
		private int _workspaceId = 12345;
		private int _integrationPointId = 98765;

		[SetUp]
		public void Setup()
		{
			_dbContext = Substitute.For<IDBContext>();
			_instance = new QueueRepository(_dbContext);
		}

		[Test]
		public void HasJobsRunningOrInQueue_True()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(5);
			
			//Act
			bool hasJobs = _instance.HasJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(hasJobs);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void HasJobsRunningOrInQueue_False()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(0);

			//Act
			bool hasJobs = _instance.HasJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsFalse(hasJobs);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void HasJobsRunning_True()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(1);

			//Act
			bool hasJobs = _instance.HasJobsExecuting(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(hasJobs);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void HasJobsRunning_False()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(0);

			//Act
			bool hasJobs = _instance.HasJobsExecuting(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsFalse(hasJobs);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}
	}
}
