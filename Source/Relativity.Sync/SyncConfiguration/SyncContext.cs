namespace Relativity.Sync.SyncConfiguration
{
	/// <inheritdoc />
	public class SyncContext : ISyncContext
	{
		/// <inheritdoc />
		public int SourceWorkspaceId { get; }

		/// <inheritdoc />
		public int DestinationWorkspaceId { get; }

		/// <inheritdoc />
		public int ParentObjectId { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceWorkspaceId"></param>
		/// <param name="destinationWorkspaceId"></param>
		/// <param name="parentObjectId"></param>
		public SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int parentObjectId)
		{
			SourceWorkspaceId = sourceWorkspaceId;
			DestinationWorkspaceId = destinationWorkspaceId;
			ParentObjectId = parentObjectId;
		}
	}
}
