using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderSourceProductionPermissionValidator : IRelativityProviderSourceProductionPermissionValidator
    {
        private readonly IProductionRepository _productionRepository;
        private readonly IAPILog _logger;

        public RelativityProviderSourceProductionPermissionValidator(IProductionRepository productionRepository, IAPILog logger)
        {
            _productionRepository = productionRepository;
            _logger = logger.ForContext<RelativityProviderSourceProductionPermissionValidator>();
        }

        public ValidationResult Validate(int sourceWorkspaceId, int sourceProductionArtifactId)
        {
            var result = new ValidationResult();
            try
            {
                _productionRepository.GetProduction(sourceWorkspaceId, sourceProductionArtifactId);;
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
