﻿using System.Data;
using System.Collections;

namespace Relativity.Sync.Transfer
{
	internal delegate void OnSourceWorkspaceDataItemReadErrorEventHandler(long completedItem, ItemLevelError itemLevelError);

	internal interface ISourceWorkspaceDataReader : IDataReader
	{
		IItemStatusMonitor ItemStatusMonitor { get; }

		event OnSourceWorkspaceDataItemReadErrorEventHandler OnItemReadError;
	}
}