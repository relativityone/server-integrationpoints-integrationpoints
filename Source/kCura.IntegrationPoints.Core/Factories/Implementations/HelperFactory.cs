using System;
using kCura.IntegrationPoints.Data;
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
			if (federatedInstanceArtifactId.HasValue && federatedInstanceArtifactId.Value > 0)
			{
				IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(sourceInstanceHelper);
				IFederatedInstanceManager federatedInstanceManager = _managerFactory.CreateFederatedInstanceManager(sourceContextContainer);
				FederatedInstanceDto federatedInstance = federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceArtifactId.Value);

				try
				{
					OAuthClientDto authClientDto = _serializer.Deserialize<OAuthClientDto>(credentials);
					IHelper targetHelper = new OAuthHelper(sourceInstanceHelper, new Uri(federatedInstance.InstanceUrl), new Uri(federatedInstance.RsapiUrl),
						new Uri(federatedInstance.KeplerUrl), authClientDto,
						_tokenProvider);
					return targetHelper;
				}
				catch (Exception ex)
				{
					sourceInstanceHelper.GetLoggerFactory().GetLogger().LogError(ex, $"Unable to find Federated instance with Id: {federatedInstanceArtifactId}");
					throw;
				}
			}

			return sourceInstanceHelper;
		}
	}
}