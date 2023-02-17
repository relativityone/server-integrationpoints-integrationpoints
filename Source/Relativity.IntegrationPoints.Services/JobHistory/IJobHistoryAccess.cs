using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public interface IJobHistoryAccess
    {
        IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IList<int> workspacesWithAccess);

        IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories,
            IDictionary<int, IList<int>> workspacesWithAccess);
    }
}
