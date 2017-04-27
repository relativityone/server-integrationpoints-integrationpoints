using System;
using System.Net;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.Core
{
	public class ServiceManagerProvider : IServiceManagerProvider
	{
		#region Fields

		private readonly IConfig _config;
		private readonly ISerializer _serializer;
		private readonly ICredentialProvider _credentialProvider;
		private readonly ITokenProvider _tokenProvider;
		
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		#endregion //Fields

		#region Constructors

		public ServiceManagerProvider(IConfigFactory configFactory, ICredentialProvider credentialProvider, 
			ISerializer serializer, ITokenProvider tokenProvider)
		{
			_config = configFactory.Create();
			_credentialProvider = credentialProvider;
			_serializer = serializer;
			_tokenProvider = tokenProvider;
		}

		#endregion //Constructors

		#region Methods

		public TManager Create<TManager, TFactory>() where TFactory : IServiceManagerFactory<TManager>, new()
		{
			WinEDDS.Config.ProgrammaticServiceURL = _config.WebApiPath;

			var cookieContainer = new CookieContainer();
			NetworkCredential credentials = _credentialProvider.Authenticate(cookieContainer);
			
			return (new TFactory()).Create(credentials, cookieContainer, _config.WebApiPath);
		}

		public TManager Create<TManager, TFactory>(int? federatedInstanceId, string federatedInstanceCredentials, 
			IFederatedInstanceManager federatedInstanceManager) 
			where TFactory : IServiceManagerFactory<TManager>, new()
		{
			TManager serviceManagerProvider;
			
			if (federatedInstanceId.HasValue)
			{
				var cookieContainer = new CookieContainer();

				var oAuthClientDto = _serializer.Deserialize<OAuthClientDto>(federatedInstanceCredentials);
				FederatedInstanceDto federatedInstanceDto =
					federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceId.Value);
				
				string token = _tokenProvider.GetExternalSystemToken(oAuthClientDto.ClientId, oAuthClientDto.ClientSecret,
					new Uri(federatedInstanceDto.InstanceUrl));
				
				NetworkCredential credentials = LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer, federatedInstanceDto.WebApiUrl); //TODO Use instead _credentialProvider.Authenticate
				
				serviceManagerProvider = (new TFactory()).Create(credentials, cookieContainer, federatedInstanceDto.WebApiUrl);
			}
			else
			{
				serviceManagerProvider = Create<TManager, TFactory>();
			}

			return serviceManagerProvider;
		}

		#endregion //Methods
	}
}
