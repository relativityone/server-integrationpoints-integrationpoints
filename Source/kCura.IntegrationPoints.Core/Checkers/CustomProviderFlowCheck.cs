using System;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    internal class CustomProviderFlowCheck : ICustomProviderFlowCheck
    {
        private readonly IRipToggleProvider _toggleProvider;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IAPILog _log;

        public CustomProviderFlowCheck(IRipToggleProvider toggleProvider, IIntegrationPointService integrationPointService, IAPILog log)
        {
            _toggleProvider = toggleProvider;
            _integrationPointService = integrationPointService;
            _log = log;
        }


        public bool ShouldBeUsed(int integrationPointId)
        {
            DestinationConfiguration destinationConfiguration = _integrationPointService.GetDestinationConfiguration(integrationPointId);

            return ShouldBeUsed(destinationConfiguration);
        }

        public bool ShouldBeUsed(DestinationConfiguration destinationConfiguration)
        {
            try
            {
                bool isToggleEnabled = _toggleProvider.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>();
                bool isManagersLinkingEnabled = destinationConfiguration.EntityManagerFieldContainsLink;
                bool isDocumentFlow = destinationConfiguration.ArtifactTypeId == (int)ArtifactType.Document;

                bool result;
                if (isDocumentFlow)
                {
                    result = isToggleEnabled;
                }
                else
                {
                    result = isToggleEnabled && !isManagersLinkingEnabled;
                }

                _log.LogInformation(
                    "Using IAPI 2.0 in Custom Providers flow: {result} because is toggle enabled: {isToggleEnabled}, is document transfer: {documentFlow}, is managers linking enabled: {isManagersLinkingEnabled}",
                    result,
                    isToggleEnabled,
                    isDocumentFlow,
                    isManagersLinkingEnabled);

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error occurred during New Custom Provider flow usage checking.");
                return false;
            }
        }

    }
}
