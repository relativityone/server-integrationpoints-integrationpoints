using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    /// <summary>
    ///     For not DataDestinationInitialization step in empty, so we don't need its configuration
    /// </summary>
    internal sealed class EmptyDataDestinationInitializationConfiguration : IDataDestinationInitializationConfiguration
    {
        public string DataDestinationName { get; } = string.Empty;

        public bool IsDataDestinationArtifactIdSet { get; } = true;

        public int DataDestinationArtifactId { get; set; }
    }
}
