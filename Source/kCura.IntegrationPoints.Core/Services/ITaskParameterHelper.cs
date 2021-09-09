using kCura.ScheduleQueue.Core;
using System;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface ITaskParameterHelper
	{
		Guid GetBatchInstance(Job job);
	}
}
