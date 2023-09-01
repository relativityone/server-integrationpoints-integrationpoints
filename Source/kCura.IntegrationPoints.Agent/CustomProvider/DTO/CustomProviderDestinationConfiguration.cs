using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderDestinationConfiguration
    {
        public int CaseArtifactId { get; private set; }

        public int ArtifactTypeId { get; private set; }

        public ImportOverwriteModeEnum ImportOverwriteMode { get; private set; }

        public string FieldOverlayBehavior { get; private set; }

        public int DestinationFolderArtifactId { get; private set; }

        public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; private set; }

        public bool UseFolderPathInformation { get; private set; }

        public int FolderPathSourceField { get; private set; }

        public bool MoveExistingDocuments { get; private set; }

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
            };
        }
    }
}
