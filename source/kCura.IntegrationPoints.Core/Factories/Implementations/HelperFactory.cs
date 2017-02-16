using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class HelperFactory : IHelperFactory
	{
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly ITokenProvider _tokenProvider;

		public HelperFactory(IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory,
			ITokenProvider tokenProvider, IIntegrationPointSerializer serializer)
		{
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_tokenProvider = tokenProvider;
			_serializer = serializer;
		}

		public IHelper CreateTargetHelper(IHelper sourceInstanceHelper, int? federatedInstanceArtifactId, string credentials)
		{
			if (federatedInstanceArtifactId.HasValue)
			{
				IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(sourceInstanceHelper);
				IFederatedInstanceManager federatedInstanceManager = _managerFactory.CreateFederatedInstanceManager(sourceContextContainer);
				FederatedInstanceDto federatedInstance = federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceArtifactId.Value);
				OAuthClientDto authClientDto = _serializer.Deserialize<OAuthClientDto>(credentials);

				IHelper targetHelper = new OAuthHelper(new Uri(federatedInstance.InstanceUrl), new Uri(federatedInstance.RsapiUrl), new Uri(federatedInstance.KeplerUrl), authClientDto,
					_tokenProvider);

				return targetHelper;
			}

			return sourceInstanceHelper;
		}
	}
}