using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class ExportProductionValidator : BasePartsValidator<ExportSettings>
    {
        private readonly IProductionManager _productionManager;

        public ExportProductionValidator(IProductionManager productionManager)
        {
            _productionManager = productionManager;
        }

        public override ValidationResult Validate(ExportSettings value)
        {
            var result = new ValidationResult();

            ProductionDTO production = _productionManager.GetProductionsForExport(value.WorkspaceId)
                .FirstOrDefault(x => x.ArtifactID.Equals(value.ProductionId.ToString()));

            if (production == null)
            {
                result.Add(FileDestinationProviderValidationMessages.PRODUCTION_NOT_EXIST);
            }

            return result;
        }
    }
}