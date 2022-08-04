namespace kCura.IntegrationPoints.Core.Contracts.Configuration
{
    public class SourceConfigurationForLogging
    {
        private readonly SourceConfiguration _sourceConfiguration;

        public SourceConfigurationForLogging(SourceConfiguration sourceConfiguration)
        {
            _sourceConfiguration = sourceConfiguration;
        }

        public int SavedSearchArtifactId => _sourceConfiguration.SavedSearchArtifactId;
        public int SourceWorkspaceArtifactId => _sourceConfiguration.SourceWorkspaceArtifactId;
        public int TargetWorkspaceArtifactId => _sourceConfiguration.TargetWorkspaceArtifactId;

        public string SavedSearch => RemoveIfNotEmpty(_sourceConfiguration.SavedSearch);

        public string SourceWorkspace => RemoveIfNotEmpty(_sourceConfiguration.SourceWorkspace);
        public string TargetWorkspace => RemoveIfNotEmpty(_sourceConfiguration.TargetWorkspace);
        public int? FederatedInstanceArtifactId => _sourceConfiguration.FederatedInstanceArtifactId;

        public SourceConfiguration.ExportType TypeOfExport => _sourceConfiguration.TypeOfExport;

        public int SourceProductionId => _sourceConfiguration.SourceProductionId;

        private string RemoveIfNotEmpty(string toSanitize)
        {
            const string sensitiveDataRemoved = "[Sensitive data has been removed]";

            return string.IsNullOrEmpty(toSanitize) ? "" : sensitiveDataRemoved;
        }
    }
}
