namespace Relativity.Sync.Configuration
{
    internal interface IDataDestinationFinalizationConfiguration : IConfiguration
    {
        int DataDestinationArtifactId { get; }
    }
}
