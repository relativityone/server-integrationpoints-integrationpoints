using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface ISavedSearchesTreeService
    {
        Task<JsTreeItemDTO> GetSavedSearchesTreeAsync(int workspaceArtifactId, int? nodeId = null, int? savedSearchId = null);
    }
}
