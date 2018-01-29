using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetTypeOfExportDefaultValueCommand : ICommand
    {
	    private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly IRelativityObjectManager _objectManager;
	    private readonly ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExpertUpdater;

	    public string SuccessMessage => "Type of Export field default value was set succesfully.";
	    public string FailureMessage => "Failed to set Type of Export field default value.";

		public SetTypeOfExportDefaultValueCommand(IIntegrationPointService integrationPointService,
		    IIntegrationPointProfileService integrationPointProfileService,
		    IRelativityObjectManager objectManager,
		    ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater)
	    {
		    _integrationPointService = integrationPointService;
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
			foreach (Data.IntegrationPoint point in _integrationPointService.GetAllRDOs())
			{
				string resultConf = _sourceConfigurationTypeOfExpertUpdater.GetCorrectedSourceConfiguration(point.SourceProvider,
					point.DestinationProvider, point.SourceConfiguration);
				if (resultConf != null)
				{
					point.SourceConfiguration = resultConf;
					_objectManager.Update(point);
				}
			}
		}

	    private void SetTypeOfExportForIntegrationPointProfiles()
	    {
		    foreach (IntegrationPointProfile profile in _integrationPointProfileService.GetAllRDOs())
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
