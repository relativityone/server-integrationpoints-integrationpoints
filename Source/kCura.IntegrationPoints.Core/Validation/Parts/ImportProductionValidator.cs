using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ImportProductionValidator : BasePartsValidator<int>
    {
        private readonly IProductionManager _productionManager;
        private readonly int _workspaceArtifactId;
        private readonly int? _federatedInstanceArtifactId;
        private readonly string _federatedInstanceCredentials;
        public ImportProductionValidator(int workspaceArtifactId, IProductionManager productionManager, int? federatedInstanceArtifactId, string credentials)
        {
            _federatedInstanceArtifactId = federatedInstanceArtifactId;
            _federatedInstanceCredentials = credentials?.ToString() ?? string.Empty;
            _workspaceArtifactId = workspaceArtifactId;
            _productionManager = productionManager;
        }

        public override ValidationResult Validate(int productionId)
        {
            var result = new ValidationResult();

            try
            {
                ProductionDTO production = _productionManager.GetProductionsForImport(_workspaceArtifactId, _federatedInstanceArtifactId, _federatedInstanceCredentials).FirstOrDefault(x => x.ArtifactID.Equals(productionId.ToString()));
                
                if (production == null)
                {
                    result.Add(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS);
                }
            }
            catch
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS);
            }

            return result;
        }
    }
}
