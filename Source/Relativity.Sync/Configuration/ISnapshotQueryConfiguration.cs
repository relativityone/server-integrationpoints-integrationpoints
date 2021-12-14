namespace Relativity.Sync.Configuration
{
    internal interface ISnapshotQueryConfiguration
    {
        int? JobHistoryToRetryId { get; }

        int DataSourceArtifactId { get; }

        int SourceWorkspaceArtifactId { get; }

        int[] ProductionImagePrecedence { get; }

        bool IncludeOriginalImageIfNotFoundInProductions { get; }
        
        int RdoArtifactTypeId { get; }
    }
}