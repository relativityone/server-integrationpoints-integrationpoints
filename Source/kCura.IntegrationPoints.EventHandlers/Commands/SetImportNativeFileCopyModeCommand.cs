using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class SetImportNativeFileCopyModeCommand : IEHCommand
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly ImportNativeFileCopyModeUpdater _importNativeFileCopyModeUpdater;

        public SetImportNativeFileCopyModeCommand(IIntegrationPointService integrationPointService,
            IIntegrationPointProfileService integrationPointProfileService,
            ImportNativeFileCopyModeUpdater importNativeFileCopyModeUpdater)
        {

            _integrationPointService = integrationPointService;
            _integrationPointProfileService = integrationPointProfileService;
            _importNativeFileCopyModeUpdater = importNativeFileCopyModeUpdater;
        }

        public void Execute()
        {
            SetImportNativeFileCopyModeForIntegrationPoints();
            SetImportNativeFileCopyModeForIntegrationPointProfiles();
        }

        private void SetImportNativeFileCopyModeForIntegrationPoints()
        {
            foreach (IntegrationPointDto point in _integrationPointService.ReadAll())
            {
                string destinationConfiguration = _importNativeFileCopyModeUpdater.GetCorrectedConfiguration(
                    point.SourceProvider,
                    point.DestinationProvider,
                    point.DestinationConfiguration);

                if (destinationConfiguration != null)
                {
                    _integrationPointService.UpdateDestinationConfiguration(point.ArtifactId, destinationConfiguration);
                }
            }
        }

        private void SetImportNativeFileCopyModeForIntegrationPointProfiles()
        {
            foreach (IntegrationPointProfileDto profile in _integrationPointProfileService.ReadAll())
            {
                string destinationConfiguration = _importNativeFileCopyModeUpdater.GetCorrectedConfiguration(
                    profile.SourceProvider,
                    profile.DestinationProvider,
                    profile.SourceConfiguration);
                if (destinationConfiguration != null)
                {
                    _integrationPointProfileService.UpdateConfiguration(profile.ArtifactId, profile.SourceConfiguration, destinationConfiguration);
                }
            }
        }
    }
}
