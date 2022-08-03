using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public interface INativesService
    {
        FileTestDto GetNativeFileInfo(int workspaceId, int documentArtifactId);
    }
}