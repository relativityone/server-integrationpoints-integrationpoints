using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs
{
	internal sealed class NotificationConfigurationStub : INotificationConfiguration
	{
		public int DestinationWorkspaceArtifactId { get; }
		public IEnumerable<string> EmailRecipients { get; }
		public int JobHistoryArtifactId { get; }
		public string JobName { get; }
		public bool SendEmails { get; }
		public int SourceWorkspaceArtifactId { get; }
		public string SourceWorkspaceTag { get; }
		public int SyncConfigurationArtifactId { get; }
	}
}