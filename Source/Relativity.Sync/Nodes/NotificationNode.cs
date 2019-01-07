using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class NotificationNode : SyncNode<INotificationConfiguration>
	{
		public NotificationNode(ICommand<INotificationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Sending notifications";
		}
	}
}