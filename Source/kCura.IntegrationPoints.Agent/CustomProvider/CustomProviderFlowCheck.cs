using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider
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

        public async Task<bool> ShouldBeUsedAsync(IntegrationPointDto integrationPoint)
        {
            try
            {
                bool isToggleEnabled = await _toggleProvider.IsEnabledAsync<EnableImportApiV2ForCustomProvidersToggle>();
                bool isManagersLinkingEnabled = integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink;

                bool result = isToggleEnabled && !isManagersLinkingEnabled;

                _log.LogInformation("Using IAPI 2.0 in Custom Providers flow: {result} because is toggle enabled: {isToggleEnabled}, is managers linking enabled: {isManagersLinkingEnabled}",
                    result, isToggleEnabled, isManagersLinkingEnabled);

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
