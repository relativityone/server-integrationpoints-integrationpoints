using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IDeleteHistoryErrorService
    {
        void DeleteErrorAssociatedWithHistories(List<int> ids, int workspaceArtifactId);
    }
}
