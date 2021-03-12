﻿namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// Represents Sync configuration context.
	/// </summary>
	public interface ISyncContext
	{
		/// <summary>
		/// Source Workspace Artifact ID.
		/// </summary>
		int SourceWorkspaceId { get; }

		/// <summary>
		/// Destination Workspace Artifact ID.
		/// </summary>
		int DestinationWorkspaceId { get; }
		
		/// <summary>
		/// Job History Artifact ID.
		/// </summary>
		int JobHistoryId { get; }
	}
}
