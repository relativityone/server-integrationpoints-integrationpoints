using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupJobHistory(InMemoryDatabase database, JobHistoryTest jobHistory)
		{
			Mock.Setup(x => x.QueryAsync(jobHistory.WorkspaceId, It.Is<QueryRequest>(r =>
					r.Condition == $"'{JobHistoryTest.BatchInstanceFieldName}' == '{jobHistory.BatchInstance}'"), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
				{
					QueryResult result = new QueryResult();

					if (database.JobHistory.FirstOrDefault(x => x.BatchInstance == jobHistory.BatchInstance) != null)
					{
						result.Objects.Add(jobHistory.ToRelativityObject());
						result.TotalCount = result.Objects.Count;
					}

					return Task.FromResult(result);
				});

			Mock.Setup(x => x.UpdateAsync(jobHistory.WorkspaceId, It.Is<UpdateRequest>(r =>
					r.Object.ArtifactID == jobHistory.ArtifactId)))
				.Returns((int workspaceId, UpdateRequest request) =>
				{
					UpdateResult result = new UpdateResult()
					{
						EventHandlerStatuses = new List<EventHandlerStatus>()
					};
					return Task.FromResult(result);
				});
		}
	}
}
