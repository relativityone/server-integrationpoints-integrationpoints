using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface IExtendedJob
	{
		long JobId { get; }
		int WorkspaceId { get; }
		int SubmittedById { get; }
		int IntegrationPointId { get; }
		IntegrationPoint IntegrationPointModel { get; }
		Guid JobIdentifier { get; }
		int JobHistoryId { get; }
	}
}