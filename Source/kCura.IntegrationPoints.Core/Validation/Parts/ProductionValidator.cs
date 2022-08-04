using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ProductionValidator : BasePartsValidator<int>
    {
        private readonly IProductionManager _productionManager;
        private readonly int _workspaceArtifactId;

        public ProductionValidator(int workspaceArtifactId, IProductionManager productionManager)
        {
            _workspaceArtifactId = workspaceArtifactId;
            _productionManager = productionManager;
        }

        public override ValidationResult Validate(int productionId)
        {
            var result = new ValidationResult();
            
            try
            {
                ProductionDTO production = _productionManager.RetrieveProduction(_workspaceArtifactId, productionId);
                if (production == null)
                {
                    result.Add(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS);
                }
            }
            catch
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS);
                return result;
            }

            return result;
        }
    }
}
