namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISyncContext
	{
		/// <summary>
		/// 
		/// </summary>
		int SourceWorkspaceId { get; }

		/// <summary>
		/// 
		/// </summary>
		int DestinationWorkspaceId { get; }

		/// <summary>
		/// 
		/// </summary>
		int ParentObjectId { get; }
	}
}
