﻿namespace Relativity.Sync.SyncConfiguration
{
	public interface ISyncContext
	{
		int SourceWorkspaceId { get; }

		int DestinationWorkspaceId { get; }

		int ParentObjectId { get; }
	}
}
