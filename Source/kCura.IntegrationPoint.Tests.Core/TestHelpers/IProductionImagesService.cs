using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public interface IProductionImagesService
    {
        IList<FileTestDto> GetProductionImagesFileInfo(int workspaceId, int documentArtifactId);
    }
}