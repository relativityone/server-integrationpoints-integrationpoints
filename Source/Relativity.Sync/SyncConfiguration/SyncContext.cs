#pragma warning disable 1591
namespace Relativity.Sync.SyncConfiguration
{
	public class SyncContext : ISyncContext
	{
		public int SourceWorkspaceId { get; }

		public int DestinationWorkspaceId { get; }
		
		public int JobHistoryId { get; }
		
		public SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId)
		{
			SourceWorkspaceId = sourceWorkspaceId;
			DestinationWorkspaceId = destinationWorkspaceId;
			JobHistoryId = jobHistoryId;
		}
	}
}
