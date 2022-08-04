using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public interface IImagesService
    {
        IList<FileTestDto> GetImagesFileInfo(int workspaceId, int documentArtifactId);
    }
}