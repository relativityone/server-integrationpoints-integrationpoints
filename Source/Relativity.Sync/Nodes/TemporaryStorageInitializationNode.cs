﻿using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class TemporaryStorageInitializationNode : SyncNode<ITemporaryStorageInitializationConfiguration>
	{
		public TemporaryStorageInitializationNode(ICommand<ITemporaryStorageInitializationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Initializing temporary storage";
		}
	}
}