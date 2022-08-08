using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public class CommonScriptsFactory : ICommonScriptsFactory
    {
        private readonly string _apiControllerName;
        private readonly ICaseServiceContext _caseServiceContext;
        private readonly IIntegrationPointBaseFieldsConstants _fieldsConstants;
        private readonly IIntegrationPointBaseFieldGuidsConstants _guidsConstants;

        public CommonScriptsFactory(IEHHelper helper, IIntegrationPointBaseFieldGuidsConstants guidsConstants, IIntegrationPointBaseFieldsConstants fieldsConstants,
            string apiControllerName)
        {
            _guidsConstants = guidsConstants;
            _fieldsConstants = fieldsConstants;
            _apiControllerName = apiControllerName;
            _caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(helper, helper.GetActiveCaseID());
        }

        //TODO: Refactor loaded scripts and extract common parts
        public ICommonScripts Create(EventHandlerBase eventHandlerBase)
        {
            int sourceProviderId = (int) eventHandlerBase.ActiveArtifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
            var sourceProviderArtifact = _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(sourceProviderId);

            if (sourceProviderArtifact.Name == Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
            {
                return CreateForExportProviders(eventHandlerBase);
            }
            return CreateForImportProviders(eventHandlerBase);
        }

        private ICommonScripts CreateForExportProviders(EventHandlerBase eventHandler)
        {
            return new RelativityProviderScripts(new ScriptsHelper(eventHandler, _caseServiceContext, _fieldsConstants, _apiControllerName), _guidsConstants,
                new FolderPathInformation(eventHandler.Helper.GetDBContext(eventHandler.Helper.GetActiveCaseID())));
        }

        private ICommonScripts CreateForImportProviders(EventHandlerBase eventHandler)
        {
            return new ImportProvidersScripts(new ScriptsHelper(eventHandler, _caseServiceContext, _fieldsConstants, _apiControllerName), _guidsConstants);
        }
    }
}