using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IResourcePoolRepository
    {
        List<ProcessingSourceLocationDTO> GetProcessingSourceLocationsByResourcePool(int resourcePoolId);
    }
}
