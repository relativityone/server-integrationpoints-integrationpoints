using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImageImportSettingsBuilder : ImportSettingsBaseBuilder<ImageSettings>
	{
		public ImageImportSettingsBuilder(IExtendedImportAPI importApi)
			: base(importApi)
		{
		}

		public override void PopulateFrom(ImportSettings importSettings, ImageSettings target)
		{
			base.PopulateFrom(importSettings, target);

			target.AutoNumberImages = importSettings.AutoNumberImages;
			target.ForProduction = importSettings.ProductionImport;
			target.ProductionArtifactID = importSettings.ProductionArtifactId;
			target.ExtractedTextEncoding = importSettings.ExtractedTextEncoding;
			target.ExtractedTextFieldContainsFilePath = importSettings.ExtractedTextFieldContainsFilePath;
			target.SelectedCasePath = importSettings.SelectedCaseFileRepoPath;
		}
	}
}
