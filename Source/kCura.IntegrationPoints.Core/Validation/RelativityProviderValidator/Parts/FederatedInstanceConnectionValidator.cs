using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class FederatedInstanceConnectionValidator : IFederatedInstanceConnectionValidator
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public FederatedInstanceConnectionValidator(IServicesMgr manager, IAPILog logger)
        {
            _servicesMgr = manager;
            _logger = logger.ForContext<RelativityProviderSourceProductionPermissionValidator>();
        }

        public ValidationResult Validate()
        {
            ValidationResult result = new ValidationResult();

            try
            {
                var workspaceManager = _servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.System);
                workspaceManager.RetrieveAllActive();
            }
            catch (Exception e)
            {
                ValidationMessage message = ValidationMessages.FederatedInstanceNotAccessible;
                result.Add(message);
                LogUnableToEstablishConnectionToFederatedInstance(e);
            }

            return result;
        }

        private void LogUnableToEstablishConnectionToFederatedInstance(Exception exception)
        {
            _logger.LogError(exception, "Unable to establish connection to federated instance");
        }
    }
}
