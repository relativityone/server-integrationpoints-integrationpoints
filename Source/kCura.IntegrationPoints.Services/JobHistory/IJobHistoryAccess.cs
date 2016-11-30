using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IJobHistoryAccess
	{
		IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IList<int> workspacesWithAccess);
	}
}