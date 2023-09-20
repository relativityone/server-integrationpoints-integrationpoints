using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderDestinationConfiguration
    {
        public int CaseArtifactId { get; set; }

        public int ArtifactTypeId { get; set; }

        public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }

        public string FieldOverlayBehavior { get; set; }

        public int DestinationFolderArtifactId { get; set; }

        public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; set; }

        public bool UseFolderPathInformation { get; set; }

        public int FolderPathSourceField { get; set; }

        public bool MoveExistingDocuments { get; set; }

        public string OverlayIdentifier { get; set; }

        public static CustomProviderDestinationConfiguration From(DestinationConfiguration configuration)
        {
            return new CustomProviderDestinationConfiguration
            {
                CaseArtifactId = configuration.CaseArtifactId,
                ArtifactTypeId = configuration.ArtifactTypeId,
                ImportOverwriteMode = configuration.ImportOverwriteMode,
                FieldOverlayBehavior = configuration.FieldOverlayBehavior,
                DestinationFolderArtifactId = configuration.DestinationFolderArtifactId,
                ImportNativeFileCopyMode = configuration.ImportNativeFileCopyMode,
                UseFolderPathInformation = configuration.UseFolderPathInformation,
                FolderPathSourceField = configuration.FolderPathSourceField,
                MoveExistingDocuments = configuration.MoveExistingDocuments,
                OverlayIdentifier = configuration.OverlayIdentifier
            };
        }
    }
}
