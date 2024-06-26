﻿namespace Relativity.Sync.Configuration
{
    internal interface IItemLevelErrorHandlerConfiguration : IConfiguration
    {
        public int SourceWorkspaceArtifactId { get; }

        public int JobHistoryArtifactId { get; }
    }
}
