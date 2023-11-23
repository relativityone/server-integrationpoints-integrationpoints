using System;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderSourceProductionPermissionValidator : IRelativityProviderSourceProductionPermissionValidator
    {
        private readonly IProductionRepository _productionRepository;
        private readonly ILogger<RelativityProviderSourceProductionPermissionValidator> _logger;

        public RelativityProviderSourceProductionPermissionValidator(IProductionRepository productionRepository, ILogger<RelativityProviderSourceProductionPermissionValidator> logger)
        {
            _productionRepository = productionRepository;
            _logger = logger;
        }

        public ValidationResult Validate(int sourceWorkspaceId, int sourceProductionArtifactId)
        {
            var result = new ValidationResult();
            try
            {
                _productionRepository.GetProduction(sourceWorkspaceId, sourceProductionArtifactId); ;
            }
            catch (Exception ex)
            {
                ValidationMessage message = ValidationMessages.SourceProductionNoAccess;
                result.Add(message);
                LogUnableToRetrieveProduction(ex);
            }

            return result;
        }

        protected virtual void LogUnableToRetrieveProduction(Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve production");
        }
    }
}
