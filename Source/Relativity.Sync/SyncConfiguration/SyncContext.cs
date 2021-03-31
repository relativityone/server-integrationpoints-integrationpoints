namespace Relativity.Sync.SyncConfiguration
{
	/// <inheritdoc cref="ISyncContext"/>
	public class SyncContext : ISyncContext
	{
		/// <inheritdoc cref="ISyncContext"/>
		public int SourceWorkspaceId { get; }

		/// <inheritdoc cref="ISyncContext"/>
		public int DestinationWorkspaceId { get; }

		/// <inheritdoc cref="ISyncContext"/>
		public int JobHistoryId { get; }
		
		/// <summary>
		/// Creates new instance of <see cref="SyncContext"/> class.
		/// </summary>
		/// <param name="sourceWorkspaceId">Specifies the source workspace Artifact ID.</param>
		/// <param name="destinationWorkspaceId">Specifies the destination workspace Artifact ID.</param>
		/// <param name="jobHistoryId">Specifies Job History Artifact ID.</param>
		public SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId)
		{
			SourceWorkspaceId = sourceWorkspaceId;
			DestinationWorkspaceId = destinationWorkspaceId;
			JobHistoryId = jobHistoryId;
		}
	}
}
