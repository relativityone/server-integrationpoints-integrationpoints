using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile
{
	public class ImportFromLoadFileModel
	{
		public ImportFromLoadFileModel(string name, string transferredObject)
		{
			General = new IntegrationPointGeneralModel(name)
			{
				Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Import,
				SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE,
				TransferredObject = transferredObject
			};
			LoadFileSettings = new ImportLoadFileSettingsModel();
			FileEncoding = new ImportLoadFileEncodingModel();
			ImageProductionSettings = new ImportLoadFileImageProductionSettingsModel();
			SharedImportSettings = new ImportSettingsModel();
			ImportDocumentSettings = new ImportDocumentSettingsModel();
		}

		public IntegrationPointGeneralModel General { get; set; }

		public ImportLoadFileSettingsModel LoadFileSettings { get; set; }

		public ImportLoadFileEncodingModel FileEncoding { get; set; }

		public ImportLoadFileImageProductionSettingsModel ImageProductionSettings { get; set; }

		public ImportSettingsModel SharedImportSettings { get; set; }

		public ImportDocumentSettingsModel ImportDocumentSettings { get; set; }
	}
}
