using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ImportProductionValidator : BasePartsValidator<int>
    {
        private readonly IPermissionManager _permissionManager;
        private readonly IProductionManager _productionManager;
        private readonly int _workspaceArtifactId;
        private readonly int? _federatedInstanceArtifactId;
        private readonly string _federatedInstanceCredentials;

        public ImportProductionValidator(int workspaceArtifactId, IProductionManager productionManager, IPermissionManager permissionManager, int? federatedInstanceArtifactId, string credentials)
        {
            _federatedInstanceArtifactId = federatedInstanceArtifactId;
            _federatedInstanceCredentials = credentials ?? string.Empty;
            _workspaceArtifactId = workspaceArtifactId;
            _productionManager = productionManager;
            _permissionManager = permissionManager;
        }

        public override ValidationResult Validate(int productionId)
        {
            var result = new ValidationResult();
            result.Add(ValidateViewPermissionForProduction(productionId));
            if (result.IsValid)
            {
                result.Add(ValidateProductionState(productionId));
            }
            if (result.IsValid)
            {
                result.Add(ValidateCreatePermissionForProductionSource(productionId));
            }
            return result;
        }

        private ValidationResult ValidateViewPermissionForProduction(int productionId)
        {
            bool isProductionAvailable = _productionManager.IsProductionInDestinationWorkspaceAvailable(_workspaceArtifactId, productionId, _federatedInstanceArtifactId, _federatedInstanceCredentials);

            return isProductionAvailable ?
                new ValidationResult() :
                new ValidationResult(ValidationMessages.MissingDestinationProductionPermissions);
        }

        private ValidationResult ValidateProductionState(int productionId)
        {
            bool isProductionInProperState = _productionManager.IsProductionEligibleForImport(_workspaceArtifactId, productionId, _federatedInstanceArtifactId, _federatedInstanceCredentials);

            return isProductionInProperState ?
                new ValidationResult() :
                new ValidationResult(ValidationMessages.DestinationProductionNotEligibleForImport);
        }

        private ValidationResult ValidateCreatePermissionForProductionSource(int productionId)
        {
            var result = new ValidationResult();
            bool canAddSubfolders = _permissionManager.UserHasArtifactInstancePermission(_workspaceArtifactId, Constants.ObjectTypeArtifactTypesGuid.ProductionDataSource, productionId, ArtifactPermission.Create);
            if (!canAddSubfolders)
            {
                result.Add(ValidationMessages.MissingDestinationProductionPermissions);
            }
            return result;
        }
    }
}
