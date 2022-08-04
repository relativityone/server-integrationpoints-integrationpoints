using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IViewService
    {
        List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId);
    }
}
