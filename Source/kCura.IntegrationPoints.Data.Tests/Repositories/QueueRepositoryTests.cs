using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class QueueRepositoryTests : TestBase
	{
		private IQueueRepository _instance;
		private IHelper _helper;
		private IDBContext _dbContext;
		private int _workspaceId = 12345;
		private int _integrationPointId = 98765;
		private readonly DateTime _runTime = new DateTime(2016, 05, 10);
		private long _jobId = 4141;

		[SetUp]
		public override void SetUp()
		{
			_dbContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(-1).Returns(_dbContext);

			_instance = new QueueRepository(_helper);

			_helper.Received().GetDBContext(-1);
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_JobsPresent_NonZero()
		{
			//Arrange
			int queuedOrRunningJobs = 5;
			int scheduledRunningJobs = 2;
			string queuedOrRunningSql = @"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is null";
			string scheduledRunningSql = @"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is not null AND [LockedByAgentID] is not null";

			IEnumerable<SqlParameter> queuedOrRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
			};
			IEnumerable<SqlParameter> scheduledRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
			};
		
			_dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(queuedOrRunningParameters, y))).Returns(queuedOrRunningJobs);
			_dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(scheduledRunningParameters, y))).Returns(scheduledRunningJobs);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == queuedOrRunningJobs + scheduledRunningJobs);
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(queuedOrRunningParameters, y)));
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(scheduledRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(scheduledRunningParameters, y)));
		}

		[Test]
		public void GetNumberOfJobsExecutingOrInQueue_NoJobs_Zero()
		{
			//Arrange
			string queuedOrRunningSql = @"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is null"; 
			string scheduledRunningSql = @"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is not null AND [LockedByAgentID] is not null";

			IEnumerable<SqlParameter> queuedOrRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
			};

			IEnumerable<SqlParameter> scheduledRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
			};

			_dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(queuedOrRunningParameters, y))).Returns(0);
			_dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(scheduledRunningParameters, y))).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(queuedOrRunningParameters, y)));
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(scheduledRunningSql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(scheduledRunningParameters, y)));
		}

		[Test]
		public void GetNumberOfJobsExecuting_JobsPresent_NonZero()
		{
			//Arrange
			int runningJobs = 1;
			string sql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [LockedByAgentID] is not null AND [NextRunTime] <= @dateValue AND [JobID] != @jobId";

			IEnumerable<SqlParameter> parameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
				new SqlParameter("@dateValue", SqlDbType.DateTime) {Value = _runTime},
				new SqlParameter("@jobId", SqlDbType.BigInt) {Value = _jobId}
			};

			_dbContext.ExecuteSqlStatementAsScalar<int>(sql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(parameters, y))).Returns(runningJobs);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

			//Assert
			Assert.IsTrue(numJobs == runningJobs);
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(sql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(parameters, y)));
		}

		[Test]
		public void GetNumberOfJobsExecuting_NoJobs_Zero()
		{
			//Arrange
			string sql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [LockedByAgentID] is not null AND [NextRunTime] <= @dateValue AND [JobID] != @jobId";

			IEnumerable<SqlParameter> parameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = _workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = _integrationPointId},
				new SqlParameter("@dateValue", SqlDbType.DateTime) {Value = _runTime},
				new SqlParameter("@jobId", SqlDbType.BigInt) {Value = _jobId}
			};

			_dbContext.ExecuteSqlStatementAsScalar<int>(sql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(parameters, y))).Returns(0);

			//Act
			int numJobs = _instance.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

			//Assert
			Assert.IsTrue(numJobs == 0);
			_dbContext.Received(1).ExecuteSqlStatementAsScalar<int>(sql, Arg.Is<IEnumerable<SqlParameter>>(y => MatchHelper.MatchesSqlParameters(parameters, y)));
		}
	}
}
