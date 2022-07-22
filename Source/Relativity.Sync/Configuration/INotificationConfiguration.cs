using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
    internal interface INotificationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        int JobHistoryArtifactId { get; }

        bool SendEmails { get; }

        int SourceWorkspaceArtifactId { get; }

        int SyncConfigurationArtifactId { get; }

        IEnumerable<string> GetEmailRecipients();

        string GetJobName();

        string GetSourceWorkspaceTag();
    }
}