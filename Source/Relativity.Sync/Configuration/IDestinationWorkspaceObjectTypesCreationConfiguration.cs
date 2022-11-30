namespace Relativity.Sync.Configuration
{
    internal interface IDestinationWorkspaceObjectTypesCreationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        bool EnableTagging { get; }
    }
}