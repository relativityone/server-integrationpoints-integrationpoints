using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using System;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public interface ITaskParametersBuilder
	{
		TaskParameters Build(TaskType taskType, Guid batchInstanceId, IntegrationPoint integrationPoint);
	}
}
