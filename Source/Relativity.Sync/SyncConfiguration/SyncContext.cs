namespace Relativity.Sync.SyncConfiguration
{
	public class SyncContext : ISyncContext
	{
		public int SourceWorkspaceId { get; }
		public int DestinationWorkspaceId { get; }
		public int ParentObjectId { get; }

		public SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int parentObjectId)
		{
			SourceWorkspaceId = sourceWorkspaceId;
			DestinationWorkspaceId = destinationWorkspaceId;
			ParentObjectId = parentObjectId;
		}
	}
}
