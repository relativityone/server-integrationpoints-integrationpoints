using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetImportNativeFileCopyModeCommand : IEHCommand
	{
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IIntegrationPointProfileService _integrationPointProfileService;
		private readonly IRelativityObjectManager _objectManager;
		private readonly ImportNativeFileCopyModeUpdater _importNativeFileCopyModeUpdater;

		public SetImportNativeFileCopyModeCommand(IIntegrationPointService integrationPointService,
			IIntegrationPointProfileService integrationPointProfileService,
			IRSAPIService service,
			ImportNativeFileCopyModeUpdater importNativeFileCopyModeUpdater)
		{

			_integrationPointService = integrationPointService;
			_integrationPointProfileService = integrationPointProfileService;
			_objectManager = service.RelativityObjectManager;
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
					_objectManager.Update(point);
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
					_objectManager.Update(profile);
				}
			}
		}
	}
}