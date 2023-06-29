using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler.RelativityServicesInterfaces
{
    public class WorkspaceManagerStub : KeplerStubBase<IWorkspaceManager>
    {
        public void SetupWorkspaceMock()
        {
            Mock.Setup(x => x.ReadAsync(It.IsAny<int>()))
                .Returns((int workspaceId) =>
                {
                    var workspaceInUse = Relativity.Workspaces.Single(x => x.ArtifactId == workspaceId);

                    return Task.FromResult(new WorkspaceResponse
                    {
                        ArtifactID = workspaceInUse.ArtifactId,
                        Name = workspaceInUse.Name,
                    });
                });
        }
    }
}
