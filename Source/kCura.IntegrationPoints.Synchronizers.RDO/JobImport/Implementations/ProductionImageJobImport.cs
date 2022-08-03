using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public class ProductionImageJobImport : ImageJobImport
    {
        private readonly IAPILog _logger;

        public ProductionImageJobImport(ImportSettings importSettings, IImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataTransferContext context, IHelper helper) :
            base(importSettings, importApi, builder, context, helper)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ProductionImageJobImport>();
        }

        protected internal override ImageImportBulkArtifactJob CreateJob()
        {
            int productionArtifactId = ImportSettings.ProductionArtifactId;
            _logger.LogInformation("Creating Production Import Job. Production ArtifactTypeId: {productionArtifactId}", productionArtifactId);
            return ImportApi.NewProductionImportJob(productionArtifactId);
        }
    }
}