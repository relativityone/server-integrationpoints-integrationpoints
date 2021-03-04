using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupWorkspace(WorkspaceTest workspace)
		{
			Mock.Setup(x => x.QuerySlimAsync(-1, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int) ArtifactType.Case &&
					q.Condition == $"'ArtifactID' == {workspace.ArtifactId}"), 0, 1))
				.Returns(() =>
				{
					var result = Database.Workspaces.Exists(x => x.ArtifactId == workspace.ArtifactId)
						? new QueryResultSlim {TotalCount = 1}
						: new QueryResultSlim();

					return Task.FromResult(result);
				});
		}
	}
}
