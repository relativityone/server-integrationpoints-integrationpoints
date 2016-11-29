using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IJobHistoryAccess
	{
		IList<Data.JobHistory> Filter(IEnumerable<Data.JobHistory> allJobHistories, IList<int> workspacesWithAccess);
	}
}