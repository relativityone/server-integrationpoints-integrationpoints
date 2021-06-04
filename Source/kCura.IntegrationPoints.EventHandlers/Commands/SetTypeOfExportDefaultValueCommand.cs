using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetTypeOfExportDefaultValueCommand : ICommand
    {
	    private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly IRelativityObjectManager _objectManager;
	    private readonly ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExpertUpdater;

	    public string SuccessMessage => "Type of Export field default value was set succesfully.";
	    public string FailureMessage => "Failed to set Type of Export field default value.";

		public SetTypeOfExportDefaultValueCommand(IIntegrationPointRepository integrationPointRepository,
		    IIntegrationPointProfileService integrationPointProfileService,
		    IRelativityObjectManager objectManager,
		    ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater)
	    {
			_integrationPointRepository = integrationPointRepository;
		    _integrationPointProfileService = integrationPointProfileService;
		    _objectManager = objectManager;
		    _sourceConfigurationTypeOfExpertUpdater = sourceConfigurationTypeOfExpertUpdater;
	    }

	    public void Execute()
        {
            SetTypeOfExportForIntegrationPoints();
            SetTypeOfExportForIntegrationPointProfiles();
        }

        private void SetTypeOfExportForIntegrationPoints()
        {
			foreach (IntegrationPoint point in _integrationPointRepository.GetIntegrationPointsWithAllFields())
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
		    foreach (IntegrationPointProfile profile in _integrationPointProfileService.GetAllRDOsWithAllFields())
		    {
			    string resultConf = _sourceConfigurationTypeOfExpertUpdater.GetCorrectedSourceConfiguration(profile.SourceProvider,
				    profile.DestinationProvider, profile.SourceConfiguration);
			    if (resultConf != null)
			    {
				    profile.SourceConfiguration = resultConf;
				    _objectManager.Update(profile);
			    }
		    }
	    }
	}
}
