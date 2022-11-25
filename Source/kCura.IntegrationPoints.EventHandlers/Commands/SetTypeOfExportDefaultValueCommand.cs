using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class SetTypeOfExportDefaultValueCommand : ICommand
    {
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExpertUpdater;

        public SetTypeOfExportDefaultValueCommand(IIntegrationPointRepository integrationPointRepository,
            IIntegrationPointProfileService integrationPointProfileService,
            ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater)
        {
            _integrationPointRepository = integrationPointRepository;
            _integrationPointProfileService = integrationPointProfileService;
            _sourceConfigurationTypeOfExpertUpdater = sourceConfigurationTypeOfExpertUpdater;
        }

        public void Execute()
        {
            SetTypeOfExportForIntegrationPoints();
            SetTypeOfExportForIntegrationPointProfiles();
        }

        private void SetTypeOfExportForIntegrationPoints()
        {
            foreach (IntegrationPoint point in _integrationPointRepository.ReadAll())
            {
                string resultConf = _sourceConfigurationTypeOfExpertUpdater.GetCorrectedSourceConfiguration(point.SourceProvider,
                    point.DestinationProvider, point.SourceConfiguration);
                if (resultConf != null)
                {
                    point.SourceConfiguration = resultConf;
                    _integrationPointRepository.Update(point);
                }
            }
        }

        private void SetTypeOfExportForIntegrationPointProfiles()
        {
            foreach (IntegrationPointProfileDto profile in _integrationPointProfileService.ReadAllSlim())
            {
                string sourceConfiguration = _sourceConfigurationTypeOfExpertUpdater.GetCorrectedSourceConfiguration(profile.SourceProvider,
                    profile.DestinationProvider, profile.SourceConfiguration);
                if (sourceConfiguration != null)
                {
                    _integrationPointProfileService.UpdateConfiguration(profile.ArtifactId, sourceConfiguration, profile.DestinationConfiguration);
                }
            }
        }
    }
}
