using Moq;
using Relativity.Services.Workspace;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class WorkspaceManagerStub : KeplerStubBase<IWorkspaceManager>
	{
		public void SetupWorkspaceMock()
		{
			Mock.Setup(x => x.GetDefaultWorkspaceFileShareResourceServerAsync(It.IsAny<WorkspaceRef>()))
				.Returns((WorkspaceRef workspace) =>
				{
					var workspaceInUse = Relativity.Workspaces.Single(x => x.ArtifactId == workspace.ArtifactID);

					return Task.FromResult(workspaceInUse.FileShareServer);
				});
		}
	}
}
