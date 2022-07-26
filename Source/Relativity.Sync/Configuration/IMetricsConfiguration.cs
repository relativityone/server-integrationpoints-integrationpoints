namespace Relativity.Sync.Configuration
{
    internal interface IMetricsConfiguration : IConfiguration
    {
        string CorrelationId { get; }
        
        string ExecutingApplication { get; }
        
        string ExecutingApplicationVersion { get; }

        DataSourceType DataSourceType { get; }

        DestinationLocationType DataDestinationType { get; }

        bool ImageImport { get; }

        int? JobHistoryToRetryId { get; }

        string SyncVersion { get; }

        int RdoArtifactTypeId { get;}

        int DestinationRdoArtifactTypeId { get; }
    }
}