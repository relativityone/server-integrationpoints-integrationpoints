using System;
using System.Linq;
using System.Net;
using System.Web.Services;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	

	public class HelperFactory : IHelperFactory
	{
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly ITokenProvider _tokenProvider;

		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

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

				OAuthHelper targetHelper = new OAuthHelper(new Uri(federatedInstance.InstanceUrl), new Uri(federatedInstance.RsapiUrl), new Uri(federatedInstance.KeplerUrl), authClientDto,
					_tokenProvider);

				return targetHelper;
			}

			return sourceInstanceHelper;
		}

		public WsInstanceInfo GetNetworkCredential(IHelper sourceInstanceHelper, int? federatedInstanceArtifactId, string credentials, CookieContainer cookieContainer)
		{
			if (federatedInstanceArtifactId.HasValue)
			{
				IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(sourceInstanceHelper);
				IFederatedInstanceManager federatedInstanceManager = _managerFactory.CreateFederatedInstanceManager(sourceContextContainer);
				FederatedInstanceDto federatedInstance = federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceArtifactId.Value);

				OAuthClientDto authClientDto = _serializer.Deserialize<OAuthClientDto>(credentials);

				var token = _tokenProvider.GetExternalSystemToken(authClientDto.ClientId, authClientDto.ClientSecret,
					new Uri(federatedInstance.InstanceUrl));

				return new WsInstanceInfo
				{
					NetworkCredential =
						RipUserManger.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer, federatedInstance.WebApiUrl),
					WebServiceUrl = federatedInstance.WebApiUrl
				};
			}
			else
			{
				var token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
				return new WsInstanceInfo
				{
					NetworkCredential =
						LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer),
					WebServiceUrl = WinEDDS.Config.WebServiceURL
				};
			}
		}
	}
}