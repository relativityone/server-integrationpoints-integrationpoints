using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateRelativityConfigurationCommand : UpdateIntegrationPointConfigurationCommandBase
	{
		private readonly IRemoveSecuredConfigurationFromIntegrationPointService _removeSecuredConfigurationService;

		public UpdateRelativityConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService, 
			IRemoveSecuredConfigurationFromIntegrationPointService removeSecuredConfigurationService) : 
			base(integrationPointForSourceService, integrationPointService)
		{
			_removeSecuredConfigurationService = removeSecuredConfigurationService;
		}

		protected override string SourceProviderGuid => Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY;

		protected override IntegrationPoint ConvertIntegrationPoint(IntegrationPoint integrationPoint)
		{
			if (string.IsNullOrEmpty(integrationPoint?.SecuredConfiguration))
			{
				return null;
			}

			return _removeSecuredConfigurationService.RemoveSecuredConfiguration(integrationPoint) ? integrationPoint : null;
		}
	}
}