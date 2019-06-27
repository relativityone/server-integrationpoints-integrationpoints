using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.ServiceProxy;
using ClientBearerTokenCredentials = kCura.Relativity.Client.BearerTokenCredentials;
using ServiceProxyBearerTokenCredentials = Relativity.Services.ServiceProxy.BearerTokenCredentials;

namespace kCura.IntegrationPoints.Core
{
	public class FederatedInstanceServicesManager : IServicesMgr
	{
		private readonly Uri _keplerUri;
		private readonly Uri _rsapiUri;
		private readonly Uri _instanceUri;
		private readonly OAuthClientDto _oAuthClientDto;
		private readonly ITokenProvider _tokenProvider;

		public FederatedInstanceServicesManager(
			Uri keplerUri, 
			Uri rsapiUri,
			Uri instanceUri,
			OAuthClientDto oAuthClientDto,
			ITokenProvider tokenProvider)
		{
			_keplerUri = keplerUri;
			_rsapiUri = rsapiUri;
			_instanceUri = instanceUri;
			_oAuthClientDto = oAuthClientDto;
			_tokenProvider = tokenProvider;
		}

		public Uri GetServicesURL() => _rsapiUri;

		public Uri GetRESTServiceUrl() => _keplerUri;

		public T CreateProxy<T>(ExecutionIdentity identity) where T : IDisposable
		{
			if (typeof(T) == typeof(IRSAPIClient))
			{
				return (T)CreateRsapiProxy();
			}

			return CreateKeplerProxy<T>(identity);
		}

		private IRSAPIClient CreateRsapiProxy()
		{
			string token = GetValidSystemToken(
				errorMessage: "Unable to connect to federated instance RSAPI"
			);

			var authenticationType = new ClientBearerTokenCredentials(token);

			return new RSAPIClient(
				_rsapiUri,
				authenticationType
			);
		}

		private T CreateKeplerProxy<T>(ExecutionIdentity identity) where T : IDisposable
		{
			string token = GetValidSystemToken(
				errorMessage: "Unable to connect to federated instance Kepler"
			);

			var credentials = new ServiceProxyBearerTokenCredentials(token);

			ServiceFactorySettings userSettings = new ServiceFactorySettings(
				_rsapiUri,
				_keplerUri,
				credentials
			);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		private string GetValidSystemToken(string errorMessage)
		{
			string token = _tokenProvider.GetExternalSystemToken(
				_oAuthClientDto.ClientId,
				_oAuthClientDto.ClientSecret,
				_instanceUri
			);
			if (string.IsNullOrEmpty(token))
			{
				throw new IntegrationPointsException(errorMessage);
			}

			return token;
		}
	}
}
