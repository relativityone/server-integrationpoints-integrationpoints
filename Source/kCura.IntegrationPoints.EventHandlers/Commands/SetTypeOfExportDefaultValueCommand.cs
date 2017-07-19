using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetTypeOfExportDefaultValueCommand : ICommand
    {
	    private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly IGenericLibrary<Data.IntegrationPoint> _integrationPointLibrary;
        private readonly IGenericLibrary<IntegrationPointProfile> _integrationPointProfileLibrary;
	    private readonly ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExpertUpdater;

	    public string SuccessMessage => "Type of Export field default value was set succesfully.";
	    public string FailureMessage => "Failed to set Type of Export field default value.";

		public SetTypeOfExportDefaultValueCommand(IIntegrationPointService integrationPointService,
		    IIntegrationPointProfileService integrationPointProfileService,
		    IGenericLibrary<Data.IntegrationPoint> integrationPointLibrary,
		    IGenericLibrary<IntegrationPointProfile> integrationPointProfileLibrary,
		    ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater)
	    {
		    _integrationPointService = integrationPointService;
		    _integrationPointProfileService = integrationPointProfileService;
		    _integrationPointLibrary = integrationPointLibrary;
		    _integrationPointProfileLibrary = integrationPointProfileLibrary;
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
					_integrationPointLibrary.Update(point);
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
				    _integrationPointProfileLibrary.Update(profile);
			    }
		    }
	    }
	}
}
