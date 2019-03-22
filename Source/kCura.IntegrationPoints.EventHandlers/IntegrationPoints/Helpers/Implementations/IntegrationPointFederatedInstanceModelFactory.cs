using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointFederatedInstanceModelFactory : IFederatedInstanceModelFactory
	{
		private readonly IIntegrationPointRepository _integrationPointRepository;

		public IntegrationPointFederatedInstanceModelFactory(IIntegrationPointRepository integrationPointRepository)
		{
			_integrationPointRepository = integrationPointRepository;
		}

		public FederatedInstanceModel Create(IDictionary<string, object> settings, Artifact artifact)
		{
			var federatedInstanceModel = new FederatedInstanceModel();
			if (settings.ContainsKey(nameof(SourceConfiguration.FederatedInstanceArtifactId)) &&
				settings[nameof(SourceConfiguration.FederatedInstanceArtifactId)] != null)
			{
				federatedInstanceModel.FederatedInstanceArtifactId = int.Parse(settings[nameof(SourceConfiguration.FederatedInstanceArtifactId)].ToString());
				string securedConfiguration = _integrationPointRepository.GetSecuredConfiguration(artifact.ArtifactID);
				federatedInstanceModel.Credentials = securedConfiguration;
			}
			return federatedInstanceModel;
		}
	}
}