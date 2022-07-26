namespace Relativity.Sync.Configuration
{
    interface IPreValidationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }
    }
}
