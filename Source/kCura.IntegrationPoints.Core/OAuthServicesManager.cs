using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.APIHelper;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoints.Core
{
	public class OAuthServicesManager : ServicesManagerBase
	{
		private readonly Uri _instanceUri;
		private readonly OAuthClientDto _oAuthClientDto;
		private readonly ITokenProvider _tokenProvider;

		public OAuthServicesManager(
			Uri instanceUri, 
			Uri rsapiUri, 
			Uri keplerUri,
			OAuthClientDto oAuthClientDto,
			ITokenProvider tokenProvider) 
			: base(rsapiUri, keplerUri)
		{
			_instanceUri = instanceUri;
			_oAuthClientDto = oAuthClientDto;
			_tokenProvider = tokenProvider;
		}

		protected override AuthenticationType GetAuthenticationType(ExecutionIdentity identity)
		{
			string token = _tokenProvider.GetExternalSystemToken(_oAuthClientDto.ClientId, _oAuthClientDto.ClientSecret, _instanceUri);
			AuthenticationType authenticationType = new kCura.Relativity.Client.BearerTokenCredentials(token);

			return authenticationType;
		}

		protected override Credentials GetKeplerCredentials(ExecutionIdentity identity)
		{
			string token = _tokenProvider.GetExternalSystemToken(_oAuthClientDto.ClientId, _oAuthClientDto.ClientSecret, _instanceUri);
			Credentials credentials = new global::Relativity.Services.ServiceProxy.BearerTokenCredentials(token);

			return credentials;
		}
	}
}