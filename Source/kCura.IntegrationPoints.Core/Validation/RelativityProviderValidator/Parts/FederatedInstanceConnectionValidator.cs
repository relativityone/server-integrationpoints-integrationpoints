using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class FederatedInstanceConnectionValidator : IFederatedInstanceConnectionValidator
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;
        private readonly Managers.IWorkspaceManager _keplerWorkspaceManager;
        public FederatedInstanceConnectionValidator(IServicesMgr servicesMgr, Managers.IWorkspaceManager workspaceManager, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _keplerWorkspaceManager = workspaceManager;
            _logger = logger.ForContext<RelativityProviderSourceProductionPermissionValidator>();
        }

        public ValidationResult Validate()
        {
            ValidationResult result = new ValidationResult();

            try
            {
                var workspaceManager = _servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.System);
                workspaceManager.RetrieveAllActive();
                _keplerWorkspaceManager.GetUserWorkspaces();
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
