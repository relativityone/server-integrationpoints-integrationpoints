using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
	public class WorkloadSizeTestImplementation
	{
		private readonly IKeplerServiceFactory _serviceFactory;
		private readonly RipApi _ripApi;

		public WorkloadSizeTestImplementation()
		{
			_serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			_ripApi = new RipApi(_serviceFactory);
		}

		public async Task<WorkloadSize> RequestWorkloadSizeFromRIPAsync()
		{
			Workload workloadSizeReturned = await _ripApi.GetWorkloadSizeAsync().ConfigureAwait(false);
			return workloadSizeReturned.Size;
		}

		public void AddMockJobToSqlTable()
		{
			SqlHelper.ExecuteSqlStatementAsDataTable(-1, InsertScheduleMockJobStatement());
		}

		public void RemoveMockJobFromSqlTable()
		{
			SqlHelper.ExecuteSqlStatementAsDataTable(-1, DeleteMockJobStatement());
		}

		private string InsertScheduleMockJobStatement()
		{
			string insertStatement =
				"INSERT INTO [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] " +
				"([RootJobID], [ParentJobID], [AgentTypeID] ,[LockedByAgentID], [WorkspaceID] ,[RelatedObjectArtifactID] ,[TaskType] " +
				",[NextRunTime] ,[LastRunTime] ,[ScheduleRuleType] ,[ScheduleRule] ,[JobDetails] ,[JobFlags] ,[SubmittedDate] " +
				",[SubmittedBy] ,[StopState] ,[Heartbeat] ) " +
				"VALUES (1 ,2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 6969, 12, 13, 14, 15, 16 )";
			return insertStatement;
		}

		private string DeleteMockJobStatement()
		{
			return "DELETE FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D] WHERE [JobDetails] = 6969";
		}
	}
}