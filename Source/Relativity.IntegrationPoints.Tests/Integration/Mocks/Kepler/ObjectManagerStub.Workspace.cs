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
		public void SetupWorkspace(InMemoryDatabase database, IEnumerable<WorkspaceTest> workspaces)
		{
			foreach (var workspace in workspaces)
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
			}
		}
	}
}
