using System;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface IExtendedJob
	{
		long JobId { get; }
		int WorkspaceId { get; }
		int IntegrationPointId { get; }
		Guid JobIdentifier { get; }
		int JobHistoryId { get; }
	}
}