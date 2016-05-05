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
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(5);
			
			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 5);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_Zero()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [ScheduleRuleType] is null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void GetNumberOfJobsExecuting_NonZero()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(1);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 1);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}

		[Test]
		public void GetNumberOfJobsExecuting_Zero()
		{
			//Arrange
			string sql = String.Format(@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {0} AND
										[RelatedObjectArtifactID] = {1} AND [LockedByAgentID] is not null", _workspaceId, _integrationPointId);
			_dbContext.ExecuteSqlStatementAsScalar<int>(sql).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received().ExecuteSqlStatementAsScalar<int>(sql);
		}
	}
}
