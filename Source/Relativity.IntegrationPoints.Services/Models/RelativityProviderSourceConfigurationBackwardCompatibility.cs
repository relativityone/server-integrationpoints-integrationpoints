namespace Relativity.IntegrationPoints.Services.Models
{
    internal class RelativityProviderSourceConfigurationBackwardCompatibility
    {
        public int SavedSearchArtifactId { get; }
        public int SourceWorkspaceArtifactId { get; }
        public int TargetWorkspaceArtifactId { get; }

        /// <summary>
        ///     This is not used - DestinationFolderArtifactId
        /// </summary>
        public int FolderArtifactId { get; }
        public int TypeOfExport { get; }

        public RelativityProviderSourceConfigurationBackwardCompatibility(RelativityProviderSourceConfiguration sourceConfiguration,
            RelativityProviderDestinationConfiguration destinationConfiguration)
        {
            SavedSearchArtifactId = sourceConfiguration.SavedSearchArtifactId;
            SourceWorkspaceArtifactId = sourceConfiguration.SourceWorkspaceArtifactId;
            TargetWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
            FolderArtifactId = destinationConfiguration.DestinationFolderArtifactId;
            TypeOfExport = sourceConfiguration.TypeOfExport;
        }
    }
}