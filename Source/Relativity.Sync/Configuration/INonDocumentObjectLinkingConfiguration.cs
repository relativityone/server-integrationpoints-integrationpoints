using System;

namespace Relativity.Sync.Configuration
{
    internal interface INonDocumentObjectLinkingConfiguration : INonDocumentSynchronizationConfiguration
    {
        Guid? ObjectLinkingSnapshotId { get; }
    }
}
