using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateLdapConfigurationCommand : IEHCommand
	{
		private IIntegrationPointService _integrationPointService;
		private IContextContainerFactory _contextContainerFactory;
		private IManagerFactory _managerFactory;
		private IEHContext _ehContext;
		private IEncryptionManager _encryptionManager;
		private IIntegrationPointSerializer _integrationPointSerializer;

		public UpdateLdapConfigurationCommand(IEHContext ehContext, IIntegrationPointService integrationPointService, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, IEncryptionManager encryptionManager, IIntegrationPointSerializer serializer)
		{
			_ehContext = ehContext;
			_integrationPointService = integrationPointService;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_encryptionManager = encryptionManager;
			_integrationPointSerializer = serializer;
		}

		public void Execute()
		{
			ISourceProviderManager sourceProviderManager = _managerFactory.CreateSourceProviderManager(_contextContainerFactory.CreateContextContainer(_ehContext.Helper));
			int ldapProviderArtifactId = sourceProviderManager.GetArtifactIdFromSourceProviderTypeGuidIdentifier(_ehContext.Helper.GetActiveCaseID(), Constants.IntegrationPoints.SourceProviders.LDAP);
			foreach (var integrationPoint in _integrationPointService.GetAllRDOsForSourceProvider(new List<int>() { ldapProviderArtifactId }).Where(ip => ip.SecuredConfiguration == null))
			{
				string decryptedConfiguration = _encryptionManager.Decrypt(integrationPoint.SourceConfiguration);
				LDAPSettings configuration = _integrationPointSerializer.Deserialize<LDAPSettings>(decryptedConfiguration);
				LDAPSecuredConfiguration securedConfiguration = _integrationPointSerializer.Deserialize<LDAPSecuredConfiguration>(decryptedConfiguration);
				integrationPoint.SourceConfiguration = _integrationPointSerializer.Serialize(configuration);
				integrationPoint.SecuredConfiguration = _integrationPointSerializer.Serialize(securedConfiguration);
				_integrationPointService.SaveIntegration(IntegrationPointModel.FromIntegrationPoint(integrationPoint));
			}
		}
	}
}
