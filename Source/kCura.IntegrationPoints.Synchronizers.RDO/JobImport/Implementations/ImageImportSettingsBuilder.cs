using kCura.IntegrationPoints.Domain;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public class ImageImportSettingsBuilder : ImportSettingsBaseBuilder<ImageSettings>
    {
        public ImageImportSettingsBuilder(IImportAPI importApi)
            : base(importApi)
        {
        }

        public override void PopulateFrom(ImportSettings importSettings, ImageSettings target)
        {
            base.PopulateFrom(importSettings, target);

            target.AutoNumberImages = importSettings.DestinationConfiguration.AutoNumberImages;
            target.ForProduction = importSettings.DestinationConfiguration.ProductionImport;
            target.ProductionArtifactID = importSettings.DestinationConfiguration.ProductionArtifactId;
            target.ExtractedTextEncoding = importSettings.ExtractedTextEncoding;
            target.ExtractedTextFieldContainsFilePath = importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath;
            target.SelectedCasePath = importSettings.DestinationConfiguration.SelectedCaseFileRepoPath;
            target.DestinationFolderArtifactID = importSettings.DestinationConfiguration.DestinationFolderArtifactId;

            target.FileNameField = Constants.SPECIAL_IMAGE_FILE_NAME_FIELD_NAME;
            target.BeginBatesFieldArtifactID = 1003667;
        }
    }
}
