namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the configuration for a source provider in Relativity integration points.
    /// </summary>
    public class RelativityProviderSourceConfiguration
    {
        /// <summary>
        /// Gets or sets the artifact ID of the saved search.
        /// </summary>
        public int SavedSearchArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the artifact ID of the source workspace.
        /// </summary>
        public int SourceWorkspaceArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the type of export.
        /// </summary>
        public int TypeOfExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use dynamic folder path.
        /// </summary>
        public bool UseDynamicFolderPath { get; set; }
    }
}
