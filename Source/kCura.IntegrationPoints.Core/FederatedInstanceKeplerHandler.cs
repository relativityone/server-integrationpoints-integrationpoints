using System;
using System.Net.Http;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.APIHelper.Handlers;
using Relativity.APIHelper.ServiceManagers;
using Relativity.APIHelper.ServiceManagers.ProxyHandlers;
using Relativity.Services.Pipeline;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoints.Core
{
	internal class FederatedInstanceKeplerHandler : KeplerHandler
	{

		private readonly Uri _instanceUri;
		private readonly OAuthClientDto _oAuthClientDto;
		private readonly ITokenProvider _tokenProvider;


		public FederatedInstanceKeplerHandler(
			Uri instanceUri,
			OAuthClientDto oAuthClientDto,
			ITokenProvider tokenProvider) 
		{
			_instanceUri = instanceUri;
			_oAuthClientDto = oAuthClientDto;
			_tokenProvider = tokenProvider;
		}
		
		protected override Credentials GetKeplerCredentials(ExecutionIdentity ident)
		{
			string token = _tokenProvider.GetExternalSystemToken(_oAuthClientDto.ClientId, _oAuthClientDto.ClientSecret, _instanceUri);
			Credentials credentials = new global::Relativity.Services.ServiceProxy.BearerTokenCredentials(token);

			return credentials;
		}
	}
}