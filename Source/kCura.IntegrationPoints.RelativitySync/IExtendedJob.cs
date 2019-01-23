using System;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface IExtendedJob
	{
		Job Job { get; }
		long JobId { get; }
		int WorkspaceId { get; }
		int IntegrationPointId { get; }
		Guid JobIdentifier { get; }
		int JobHistoryId { get; }
	}
}