namespace Relativity.Sync.Configuration
{
    interface INonDocumentSynchronizationConfiguration : ISynchronizationConfiguration
    {
        // add config required for object 
        int DestinationRdoArtifactTypeId { get; }
    }
}
