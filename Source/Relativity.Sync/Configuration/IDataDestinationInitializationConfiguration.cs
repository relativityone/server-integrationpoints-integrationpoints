namespace Relativity.Sync.Configuration
{
    internal interface IDataDestinationInitializationConfiguration : IConfiguration
    {
        string DataDestinationName { get; }

        bool IsDataDestinationArtifactIdSet { get; }

        int DataDestinationArtifactId { get; set; }
    }
}