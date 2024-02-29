namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the configuration for a destination provider in Relativity integration points.
    /// </summary>
    public class RelativityProviderDestinationConfiguration
    {
        /// <summary>
        /// Gets or sets the artifact type ID.
        /// </summary>
        public int ArtifactTypeID { get; set; }

        /// <summary>
        /// Gets or sets the destination artifact type ID.
        /// </summary>
        public int DestinationArtifactTypeID { get; set; }

        /// <summary>
        /// Gets or sets the case artifact ID.
        /// </summary>
        public int CaseArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the destination folder artifact ID.
        /// </summary>
        public int DestinationFolderArtifactId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to import native files.
        /// </summary>
        public bool ImportNativeFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use folder path information.
        /// </summary>
        public bool UseFolderPathInformation { get; set; }

        /// <summary>
        /// Gets or sets the folder path source field.
        /// </summary>
        public int FolderPathSourceField { get; set; }

        /// <summary>
        /// Gets or sets the field overlay behavior.
        /// </summary>
        public string FieldOverlayBehavior { get; set; }
    }
}
