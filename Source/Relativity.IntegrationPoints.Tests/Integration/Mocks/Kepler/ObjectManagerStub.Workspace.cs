using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupWorkspace(RelativityInstanceTest database, WorkspaceTest workspace)
		{
			Mock.Setup(x => x.QuerySlimAsync(-1, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.Case &&
					q.Condition == $"'ArtifactID' == {workspace.ArtifactId}"), 0, 1))
				.Returns(() =>
				{
					var result = database.Workspaces.FirstOrDefault(
							x => x.ArtifactId == workspace.ArtifactId) != null
						? new QueryResultSlim { TotalCount = 1 }
						: new QueryResultSlim();

					return Task.FromResult(result);
				});

			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int) ArtifactType.Case &&
					q.Fields.Single().Name == WorkspaceFieldsConstants.NAME_FIELD &&
					q.Condition == $"'ArtifactID' == {workspace.ArtifactId}"), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(() =>
				{
					var objects = database.Workspaces
						.Where(x => x.ArtifactId == workspace.ArtifactId)
						.Select(x => x.ToRelativityObject())
						.ToList();

					return Task.FromResult(new QueryResult
					{
						Objects = objects,
					});
				});
		}
	}
}
