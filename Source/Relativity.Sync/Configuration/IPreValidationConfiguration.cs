namespace Relativity.Sync.Configuration
{
    internal interface IPreValidationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }
    }
}
