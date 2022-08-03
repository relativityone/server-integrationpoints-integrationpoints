using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public class ScriptsHelper : IScriptsHelper
    {
        private readonly string _apiControllerName;
        private readonly ICaseServiceContext _context;
        private readonly IIntegrationPointBaseFieldsConstants _fieldsConstants;
        private readonly EventHandlerBase _handler;

        public ScriptsHelper(EventHandlerBase handler, ICaseServiceContext context, IIntegrationPointBaseFieldsConstants fieldsConstants, string apiControllerName)
        {
            _handler = handler;
            _context = context;
            _fieldsConstants = fieldsConstants;
            _apiControllerName = apiControllerName;
        }

        public int GetArtifactIdByGuid(string guid)
        {
            return _handler.GetArtifactIdByGuid(Guid.Parse(guid));
        }

        public string GetApplicationPath()
        {
            return PageInteractionHelper.GetApplicationRelativeUri();
        }

        public int GetApplicationId()
        {
            return _handler.Application.ArtifactID;
        }

        public int GetActiveArtifactId()
        {
            return _handler.ActiveArtifact.ArtifactID;
        }

        public string GetDestinationConfiguration()
        {
            return _handler.ActiveArtifact.Fields[_fieldsConstants.DestinationConfiguration].Value.Value.ToString();
        }

        public string GetSourceConfiguration()
        {
            return _handler.ActiveArtifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString();
        }

        public string GetSourceViewUrl()
        {
            int sourceProviderId = (int) _handler.ActiveArtifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
            Guid[] fieldsGuids = {Guid.Parse(SourceProviderFieldGuids.ViewConfigurationUrl)};
            SourceProvider provider = _context.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(sourceProviderId, fieldsGuids);
            return provider.ViewConfigurationUrl;
        }

        public string GetAPIControllerName()
        {
            return _apiControllerName;
        }
    }
}