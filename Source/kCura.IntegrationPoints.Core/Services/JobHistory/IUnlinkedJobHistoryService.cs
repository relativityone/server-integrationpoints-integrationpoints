using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IUnlinkedJobHistoryService
    {
        List<int> FindUnlinkedJobHistories(int workspaceArtifactId);
    }
}
