using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IResourcePoolManager
    {
        List<ProcessingSourceLocationDTO> GetProcessingSourceLocation(int workspaceId);
    }
}
