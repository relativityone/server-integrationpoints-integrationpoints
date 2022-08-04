using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity
{
    public class RenameCustodianToEntityInIntegrationPointConfigurationCommand : IEHCommand
    {
        private readonly IEHHelper _helper;
        private readonly IRelativityObjectManager _objectManager;

        private string[] _sourceProviderWithEntityObjectType => new[]
        {
            Constants.IntegrationPoints.SourceProviders.LDAP,
            Constants.IntegrationPoints.SourceProviders.FTP,
            Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE
        };

        public RenameCustodianToEntityInIntegrationPointConfigurationCommand(IEHHelper helper, IRelativityObjectManager objectManager)
        {
            _helper = helper;
            _objectManager = objectManager;
        }

        public void Execute()
        {
            foreach (string sourceProviderGuid in _sourceProviderWithEntityObjectType)
            {
                var updateCommand = new RenameCustodianToEntityForSourceProviderCommand(sourceProviderGuid, _helper, _objectManager);
                updateCommand.Execute();
            }
        }
    }
}
