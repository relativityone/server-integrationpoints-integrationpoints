using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderFlowCheck : ICustomProviderFlowCheck
    {
        private readonly IToggleProvider _toggleProvider;
        private readonly ISerializer _serializer;
        private readonly IAPILog _log;

        public CustomProviderFlowCheck(
            IToggleProvider toggleProvider,
            ISerializer serializer,
            IAPILog log)
        {
            _toggleProvider = toggleProvider;
            _serializer = serializer;
            _log = log;
        }

        public async Task<bool> ShouldBeUsedAsync(IntegrationPointDto integrationPoint)
        {
            try
            {
                bool isToggleEnabled = await _toggleProvider.IsEnabledAsync<EnableImportApiV2ForCustomProvidersToggle>();
                bool isManagersLinkingEnabled = IsManagersLinkingEnabled(integrationPoint.DestinationConfiguration);

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

        private bool IsManagersLinkingEnabled(string configuration)
        {
            ImportSettings settings = _serializer.Deserialize<ImportSettings>(configuration);
            return settings.EntityManagerFieldContainsLink;
        }
    }
}
