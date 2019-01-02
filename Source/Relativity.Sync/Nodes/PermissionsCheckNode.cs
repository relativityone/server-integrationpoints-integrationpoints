using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class PermissionsCheckNode : SyncNode<IPermissionsCheckConfiguration>
	{
		public PermissionsCheckNode(ICommand<IPermissionsCheckConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Checking permissions";
		}
	}
}