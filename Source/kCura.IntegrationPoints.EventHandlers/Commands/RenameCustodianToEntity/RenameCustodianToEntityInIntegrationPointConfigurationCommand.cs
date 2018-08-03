using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity
{
	public class RenameCustodianToEntityInIntegrationPointConfigurationCommand : IEHCommand
	{
		private readonly IIntegrationPointForSourceService _integrationPointForSourceService;
		private readonly IIntegrationPointService _integrationPointService;

		private string[] _sourceProviderWithEntityObjectType => new[]
		{
			Constants.IntegrationPoints.SourceProviders.LDAP,
			Constants.IntegrationPoints.SourceProviders.FTP,
			Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE
		};

		public RenameCustodianToEntityInIntegrationPointConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService)
		{
			_integrationPointForSourceService = integrationPointForSourceService;
			_integrationPointService = integrationPointService;
		}

		public void Execute()
		{
			foreach (string sourceProviderGuid in _sourceProviderWithEntityObjectType)
			{
				var updateCommand = new RenameCustodianToEntityForSourceProviderCommand(sourceProviderGuid,
					_integrationPointForSourceService, _integrationPointService);
				updateCommand.Execute();
			}
		}
	}
}
