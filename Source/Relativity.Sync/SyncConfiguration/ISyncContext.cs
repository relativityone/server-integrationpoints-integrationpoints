using System;

namespace Relativity.Sync.SyncConfiguration
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
		
		/// <summary>
		/// Name of executing application
		/// </summary>
		string ExecutingApplication { get; }
		
		/// <summary>
		/// Version of the executing application
		/// </summary>
		Version ExecutingApplicationVersion { get; }

		/// <summary>
		/// Specifies whether JobHistoryError RDOs should be created for item level errors
		/// </summary>
		bool LogItemLevelErrors { get; }
	}
}
