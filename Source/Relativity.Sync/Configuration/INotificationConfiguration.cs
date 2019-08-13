using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface INotificationConfiguration : IConfiguration
	{
		int DestinationWorkspaceArtifactId { get; }

		IEnumerable<string> EmailRecipients { get; }

		int JobHistoryArtifactId { get; }

		string JobName { get; }

		bool SendEmails { get; }

		int SourceWorkspaceArtifactId { get; }

		string SourceWorkspaceTag { get; }

		int SyncConfigurationArtifactId { get; }
	}
}