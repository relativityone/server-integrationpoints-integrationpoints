using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
	public class ImageRelativityToRelativityImportSettingsBuilder : ImageImportSettingsBuilder
	{
		public ImageRelativityToRelativityImportSettingsBuilder(IImportAPI importApi)
			: base(importApi)
		{
		}

		public override void PopulateFrom(ImportSettings importSettings, ImageSettings target)
		{
			base.PopulateFrom(importSettings, target);
			target.DocumentIdentifierField = importSettings.DestinationIdentifierField;
			target.BatesNumberField = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME;
			target.FileLocationField = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME;
			target.DisableImageTypeValidation = true;
			target.AutoNumberImages = true;
			target.FileNameField = Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD_NAME;
		}
	}
}