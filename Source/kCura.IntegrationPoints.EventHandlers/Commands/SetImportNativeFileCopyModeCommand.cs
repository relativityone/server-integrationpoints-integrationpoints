using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetImportNativeFileCopyModeCommand : IEHCommand
	{
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IIntegrationPointProfileService _integrationPointProfileService;
		private readonly IGenericLibrary<IntegrationPoint> _integrationPointLibrary;
		private readonly IGenericLibrary<IntegrationPointProfile> _integrationPointProfileLibrary;
		private readonly ImportNativeFileCopyModeUpdater _importNativeFileCopyModeUpdater;

		public SetImportNativeFileCopyModeCommand(IIntegrationPointService integrationPointService,
			IIntegrationPointProfileService integrationPointProfileService,
			IRSAPIService service,
			ImportNativeFileCopyModeUpdater importNativeFileCopyModeUpdater)
		{

			_integrationPointService = integrationPointService;
			_integrationPointProfileService = integrationPointProfileService;
			_integrationPointLibrary = service.IntegrationPointLibrary;
			_integrationPointProfileLibrary = service.IntegrationPointProfileLibrary;
			_importNativeFileCopyModeUpdater = importNativeFileCopyModeUpdater;
		}

		public void Execute()
		{
			SetImportNativeFileCopyModeForIntegrationPoints();
			SetImportNativeFileCopyModeForIntegrationPointProfiles();
		}

		private void SetImportNativeFileCopyModeForIntegrationPoints()
		{
			foreach (IntegrationPoint point in _integrationPointService.GetAllRDOs())
			{
				string resultConf = _importNativeFileCopyModeUpdater.GetCorrectedConfiguration(point.SourceProvider,
					point.DestinationProvider, point.DestinationConfiguration);
				if (resultConf != null)
				{
					point.DestinationConfiguration = resultConf;
					_integrationPointLibrary.Update(point);
				}
			}
		}

		private void SetImportNativeFileCopyModeForIntegrationPointProfiles()
		{
			foreach (IntegrationPointProfile profile in _integrationPointProfileService.GetAllRDOs())
			{
				string resultConf = _importNativeFileCopyModeUpdater.GetCorrectedConfiguration(profile.SourceProvider,
					profile.DestinationProvider, profile.SourceConfiguration);
				if (resultConf != null)
				{
					profile.DestinationConfiguration = resultConf;
					_integrationPointProfileLibrary.Update(profile);
				}
			}
		}
	}
}