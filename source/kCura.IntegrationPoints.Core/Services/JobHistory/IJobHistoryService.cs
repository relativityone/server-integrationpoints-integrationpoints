using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IJobHistoryService
	{
		Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc);

		IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactids);
	}
}