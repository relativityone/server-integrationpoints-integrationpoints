using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class PreValidationNode: SyncNode<IPreValidationConfiguration>
	{
		public PreValidationNode(ICommand<IPreValidationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "PreValidating";
		}
	}
}
