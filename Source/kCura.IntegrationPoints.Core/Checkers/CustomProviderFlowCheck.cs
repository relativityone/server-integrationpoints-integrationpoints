using System;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Models;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    internal class CustomProviderFlowCheck : ICustomProviderFlowCheck
    {
        private readonly IRipToggleProvider _toggleProvider;
        private readonly IAPILog _log;

        public CustomProviderFlowCheck(IRipToggleProvider toggleProvider, IAPILog log)
        {
            _toggleProvider = toggleProvider;
            _log = log;
        }

        public bool ShouldBeUsed(IntegrationPointDto integrationPoint)
        {
            try
            {
                bool isToggleEnabled = _toggleProvider.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>();
                bool isManagersLinkingEnabled = integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink;
                bool isDocumentFlow = integrationPoint.DestinationConfiguration.ArtifactTypeId == (int)ArtifactType.Document;

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
