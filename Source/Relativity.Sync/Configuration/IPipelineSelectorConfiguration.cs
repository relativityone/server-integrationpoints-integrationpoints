namespace Relativity.Sync.Configuration
{
    internal interface IPipelineSelectorConfiguration : IConfiguration
    {
        int? JobHistoryToRetryId { get; }
        bool IsImageJob { get; }
        int RdoArtifactTypeId { get; }
    }
}