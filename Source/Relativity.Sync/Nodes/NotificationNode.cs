using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class NotificationNode : SyncNode<INotificationConfiguration>
	{
		public NotificationNode(ICommand<INotificationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Sending notifications";
	}
}