using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ProductionImageJobImport : ImageJobImport
	{
		public ProductionImageJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataTransferContext context) : base(importSettings, importApi, builder, context)
		{
		}

		protected internal override ImageImportBulkArtifactJob CreateJob()
		{
			return ImportApi.NewProductionImportJob(ImportSettings.ProductionArtifactId);
		}
	}
}