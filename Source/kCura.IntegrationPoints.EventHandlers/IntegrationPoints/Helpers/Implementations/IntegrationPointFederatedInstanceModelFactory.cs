using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointFederatedInstanceModelFactory : IFederatedInstanceModelFactory
	{
		private readonly ICaseServiceContext _context;

		public IntegrationPointFederatedInstanceModelFactory(ICaseServiceContext context)
		{
			_context = context;
		}

		public FederatedInstanceModel Create(IDictionary<string, object> settings, Artifact artifact)
		{
			var federatedInstanceModel = new FederatedInstanceModel();
			if (settings.ContainsKey(nameof(SourceConfiguration.FederatedInstanceArtifactId)) &&
				settings[nameof(SourceConfiguration.FederatedInstanceArtifactId)] != null)
			{
				federatedInstanceModel.FederatedInstanceArtifactId = int.Parse(settings[nameof(SourceConfiguration.FederatedInstanceArtifactId)].ToString());
				var integrationPoint = _context.RsapiService.RelativityObjectManager.Read<IntegrationPoint>(artifact.ArtifactID);
				federatedInstanceModel.Credentials = integrationPoint.SecuredConfiguration;
			}
			return federatedInstanceModel;
		}
	}
}