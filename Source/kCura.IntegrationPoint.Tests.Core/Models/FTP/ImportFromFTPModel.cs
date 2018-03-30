using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.FTP
{
	public class ImportFromFTPModel
	{
		public ImportFromFTPModel(string name, string transferredObject)
		{
			General = new IntegrationPointGeneralModel(name)
			{
				Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Import,
				SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_FTP,
				TransferredObject = transferredObject
			};
			SharedImportSettings = new ImportSettingsModel();
		}

		public IntegrationPointGeneralModel General { get; set; }

		public ImportSettingsModel SharedImportSettings { get; set; }

		public ImportCustodianSettingsModel ImportCustodianSettings { get; set; }

		public ImportFromFTPConnectionAndFileInfoModel ConnectionAndFileInfo { get; set; }
	}
}
