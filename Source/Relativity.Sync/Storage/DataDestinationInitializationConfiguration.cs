using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class DataDestinationInitializationConfiguration : IDataDestinationInitializationConfiguration
    {
        public string DataDestinationName { get; } = string.Empty;

        public bool IsDataDestinationArtifactIdSet { get; } = false;

        public int DataDestinationArtifactId { get; set; }
    }
}
