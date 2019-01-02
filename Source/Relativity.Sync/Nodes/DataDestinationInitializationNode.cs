using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DataDestinationInitializationNode : SyncNode<IDataDestinationInitializationConfiguration>
	{
		public DataDestinationInitializationNode(ICommand<IDataDestinationInitializationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Initializing data destination";
		}
	}
}