namespace kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile
{
	public class ImportFromLoadFileModel
	{
		public ImportFromLoadFileModel(string name, string transferredObject)
		{
			General = new IntegrationPointGeneralModel(name)
			{
				Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Import,
				SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_FTP,
				TransferredObject = transferredObject
			};
			LoadFileSettings = new ImportLoadFileSettingsModel();
		}

		public IntegrationPointGeneralModel General { get; set; }

		public ImportLoadFileSettingsModel LoadFileSettings { get; set; }
	}
}
