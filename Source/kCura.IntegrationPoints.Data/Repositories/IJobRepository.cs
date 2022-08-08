using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IJobRepository
    {
        IList<RelativityObject> GetRunningJobs(int workspaceArtifactId);
        IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId);
    }
}