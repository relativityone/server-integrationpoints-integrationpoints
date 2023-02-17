namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
    public class DestinationConfigurationPermissionValidationModel // TODO refactor IP model (make Destination configuration strongly typed), then remove this class
    {
        public int ArtifactTypeId { get; set; }

        public int DestinationArtifactTypeId { get; set; }

        public int CaseArtifactId { get; set; }

        public int DestinationFolderArtifactId { get; set; }

        public bool MoveExistingDocuments { get; set; }

        public bool UseFolderPathInformation { get; set; }

        public bool UseDynamicFolderPath { get; set; }

        public bool UseFolderPath => UseFolderPathInformation || UseDynamicFolderPath;
    }
}
