using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DataDestinationFinalizationNode : SyncNode<IDataDestinationFinalizationConfiguration>
	{
		public DataDestinationFinalizationNode(ICommand<IDataDestinationFinalizationConfiguration> command, IAPILog logger) : base(command, logger)
		{
			Id = "Finalizing data destination";
		}
	}
}
