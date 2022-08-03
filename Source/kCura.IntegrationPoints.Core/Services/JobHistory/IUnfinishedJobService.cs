using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IUnfinishedJobService
    {
        IList<Data.JobHistory> GetUnfinishedJobs(int workspaceArtifactIdS);
    }
}