using System;

namespace Relativity.Sync.Tests.Common
{
    public static class FakeHelper
    {
        public static SyncJobParameters CreateSyncJobParameters(
            int syncConfigurationArtifactId = default,
            int workspaceId = default,
            int userId = default,
            Guid workflowId = default)
        {
            return new SyncJobParameters(
                syncConfigurationArtifactId,
                workspaceId,
                userId,
                workflowId, Guid.Empty);
        }
    }
}
