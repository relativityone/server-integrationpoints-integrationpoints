using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class HelperFactory : IHelperFactory
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ITokenProvider _tokenProvider;

		public HelperFactory(IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory, ITokenProvider tokenProvider)
		{
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_tokenProvider = tokenProvider;
		}

		public IHelper CreateOAuthClientHelper(IHelper sourceInstanceHelper, int federatedInstanceArtifactId)
		{
			IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(sourceInstanceHelper);
			IFederatedInstanceManager federatedInstanceManager =
				_managerFactory.CreateFederatedInstanceManager(sourceContextContainer);
			FederatedInstanceDto federatedInstance = federatedInstanceManager.RetrieveFederatedInstance(federatedInstanceArtifactId);
			IOAuthClientManager oAuthClientManager = _managerFactory.CreateOAuthClientManager(sourceContextContainer);
			OAuthClientDto oAuthClientDto =
				oAuthClientManager.RetrieveOAuthClientForFederatedInstance(federatedInstanceArtifactId);

			IHelper targetHelper = new OAuthHelper(
				new Uri(federatedInstance.InstanceUrl),
				new Uri(federatedInstance.RsapiUrl),
				new Uri(federatedInstance.KeplerUrl),
				oAuthClientDto,
				_tokenProvider);

			return targetHelper;
		}
	}
}