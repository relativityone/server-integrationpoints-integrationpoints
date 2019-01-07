using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class ValidationNode : SyncNode<IValidationConfiguration>
	{
		public ValidationNode(ICommand<IValidationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Validating";
		}
	}
}