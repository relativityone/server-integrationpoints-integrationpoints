using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.System.Core.Runner
{
    internal sealed class SyncDataAndUserConfiguration : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration,
        IUserContextConfiguration
    {
        public SyncDataAndUserConfiguration(int executingUserId)
        {
            ExecutingUserId = executingUserId;
        }

        public SyncDataAndUserConfiguration(int submittedBy, int destinationFolderArtifactId)
        {
            DataDestinationArtifactId = destinationFolderArtifactId;
            ExecutingUserId = submittedBy;
        }

        public int DataDestinationArtifactId { get; set; }

        public int ExecutingUserId { get; }

        // Currently unused properties
        public string DataDestinationName => string.Empty;

        public bool IsDataDestinationArtifactIdSet => false;
    }
}
