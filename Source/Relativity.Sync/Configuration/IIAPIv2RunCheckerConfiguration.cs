namespace Relativity.Sync.Configuration
{
    internal interface IIAPIv2RunCheckerConfiguration : IConfiguration
    {
        ImportNativeFileCopyMode NativeBehavior { get; }

        bool ImageImport { get; }

        int RdoArtifactTypeId { get; }

        bool IsRetried { get; }

        bool IsDrainStopped { get; }

        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        bool EnableTagging { get; }
    }
}
