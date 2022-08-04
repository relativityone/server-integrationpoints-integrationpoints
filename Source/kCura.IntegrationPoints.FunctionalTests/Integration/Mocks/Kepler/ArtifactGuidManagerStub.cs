using Moq;
using Relativity.Services.ArtifactGuid;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ArtifactGuidManagerStub : KeplerStubBase<IArtifactGuidManager>
    {
        public void SetupArtifactGuidManager()
        {
            Mock.Setup(x => x.ReadSingleArtifactIdAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns((int workspaceId, Guid fieldGuid) =>
                {
                    return Task.FromResult(
                        Relativity.Workspaces.Single(x => x.ArtifactId == workspaceId)
                            .Fields.Single(x => x.Guid == fieldGuid)
                            .ArtifactId);
                });
        }
    }
}
