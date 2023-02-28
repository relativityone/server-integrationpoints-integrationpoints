namespace Relativity.IntegrationPoints.Services.Models
{
    internal class RelativityProviderDestinationConfigurationBackwardCompatibility
    {
        public int ArtifactTypeID { get; set; }

        public int DestinationArtifactTypeID { get; set; }

        /// <summary>
        ///     Why do we need this?
        /// </summary>
        public string DestinationProviderType { get; set; }

        /// <summary>
        ///     The same as TargetWorkspaceId in source configuration
        /// </summary>
        public int CaseArtifactId { get; set; }

        /// <summary>
        ///     ???
        /// </summary>
        public int DestinationFolderArtifactId { get; set; }

        /// <summary>
        ///     Why do we need this?
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        ///     Used only on UI
        /// </summary>
        public bool DoNotUseFieldsMapCache { get; set; }

        /// <summary>
        ///     The same as OverwriteFieldsChoiceId in IntegrationPoint
        /// </summary>
        public string ImportOverwriteMode { get; set; }

        public bool ImportNativeFile { get; set; }

        public bool UseFolderPathInformation { get; set; }

        public bool UseDynamicFolderPath { get; set; }

        public int FolderPathSourceField { get; set; }

        /// <summary>
        ///     Hardcoded false as Relativity Provider doesn't use Extracted Text Files
        /// </summary>
        public bool ExtractedTextFieldContainsFilePath { get; set; }

        /// <summary>
        ///     Hardcoded utf-16
        /// </summary>
        public string ExtractedTextFileEncoding { get; set; }

        /// <summary>
        ///     Hardcoded true as long as we cannot export Custodian RDO
        /// </summary>
        public bool EntityManagerFieldContainsLink { get; set; }

        public string FieldOverlayBehavior { get; set; }

        public RelativityProviderDestinationConfigurationBackwardCompatibility(RelativityProviderDestinationConfiguration destinationConfiguration,
            RelativityProviderSourceConfiguration sourceConfiguration, string overwriteFieldsChoice)
        {
            ArtifactTypeID = destinationConfiguration.ArtifactTypeID;
            DestinationArtifactTypeID = destinationConfiguration.DestinationArtifactTypeID != 0 ? destinationConfiguration.DestinationArtifactTypeID : destinationConfiguration.ArtifactTypeID;
            DestinationProviderType = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID;
            CaseArtifactId = destinationConfiguration.CaseArtifactId;
            DestinationFolderArtifactId = destinationConfiguration.DestinationFolderArtifactId;
            Provider = "relativity";
            DoNotUseFieldsMapCache = false;
            ImportOverwriteMode = overwriteFieldsChoice.Replace("/", string.Empty).Replace(" ", string.Empty);
            ImportNativeFile = destinationConfiguration.ImportNativeFile;
            UseFolderPathInformation = destinationConfiguration.UseFolderPathInformation;
            FolderPathSourceField = destinationConfiguration.FolderPathSourceField;
            ExtractedTextFieldContainsFilePath = false;
            ExtractedTextFileEncoding = "utf-16";
            EntityManagerFieldContainsLink = true;
            FieldOverlayBehavior = destinationConfiguration.FieldOverlayBehavior;
            UseDynamicFolderPath = sourceConfiguration.UseDynamicFolderPath;
        }
    }
}
