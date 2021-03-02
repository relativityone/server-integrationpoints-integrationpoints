using System.Linq;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupWorkspace(Workspace workspace)
		{
			Mock.Setup(x => x.QuerySlimAsync(-1, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int) ArtifactType.Case &&
					q.Condition == $"'ArtifactID' == {workspace.ArtifactId}"), 0, 1))
				.ReturnsAsync(new QueryResultSlim()
				{
					TotalCount = Database.Workspaces.Count(x => x.ArtifactId == workspace.ArtifactId)
				});
		}
	}
}
