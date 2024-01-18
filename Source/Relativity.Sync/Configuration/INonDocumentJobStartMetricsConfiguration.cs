namespace Relativity.Sync.Configuration
{
    internal interface INonDocumentJobStartMetricsConfiguration : IJobStartMetricsConfiguration
    {
        int RdoArtifactTypeId { get; }
    }
}
