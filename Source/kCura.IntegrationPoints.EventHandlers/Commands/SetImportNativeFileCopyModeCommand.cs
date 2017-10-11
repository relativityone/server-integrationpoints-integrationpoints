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
			IGenericLibrary<IntegrationPoint> integrationPointLibrary,
			IGenericLibrary<IntegrationPointProfile> integrationPointProfileLibrary,
			ImportNativeFileCopyModeUpdater importNativeFileCopyModeUpdater)
		{
			_integrationPointService = integrationPointService;
			_integrationPointProfileService = integrationPointProfileService;
			_integrationPointLibrary = integrationPointLibrary;
			_integrationPointProfileLibrary = integrationPointProfileLibrary;
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
				string resultConf = _importNativeFileCopyModeUpdater.GetCorrectedSourceConfiguration(point.SourceProvider,
					point.DestinationProvider, point.SourceConfiguration);
				if (resultConf != null)
				{
					point.SourceConfiguration = resultConf;
					_integrationPointLibrary.Update(point);
				}
			}
		}

		private void SetImportNativeFileCopyModeForIntegrationPointProfiles()
		{
			foreach (IntegrationPointProfile profile in _integrationPointProfileService.GetAllRDOs())
			{
				string resultConf = _importNativeFileCopyModeUpdater.GetCorrectedSourceConfiguration(profile.SourceProvider,
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