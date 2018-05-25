using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.APIHelper.ServiceManagers.ProxyHandlers;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.APIHelper.ServiceManagers;

namespace kCura.IntegrationPoints.Core
{
	internal class FederatedInstanceRsapiHandler : RSAPIClientHandler
	{
		private readonly ITokenProvider _tokenProvider;
		private readonly OAuthClientDto _oAuthClientDto;
		private readonly Uri _instanceUri;

		public FederatedInstanceRsapiHandler(
			Uri instanceUri,
			OAuthClientDto oAuthClientDto,
			ITokenProvider tokenProvider)
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
	}
}