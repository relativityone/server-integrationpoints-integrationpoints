using System;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IJobHistoryService
	{
		Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUTC);
	}
}