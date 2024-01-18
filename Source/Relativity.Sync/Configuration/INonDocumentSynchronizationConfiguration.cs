namespace Relativity.Sync.Configuration
{
    internal interface INonDocumentSynchronizationConfiguration : ISynchronizationConfiguration
    {
        // add config required for object
        int DestinationRdoArtifactTypeId { get; }
    }
}
