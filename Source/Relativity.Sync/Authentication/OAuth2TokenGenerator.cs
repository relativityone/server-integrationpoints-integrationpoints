using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using kCura.WinEDDS.Credentials;
using Relativity.OAuth2Client.Interfaces;

namespace Relativity.Sync.Authentication
{
	internal sealed class OAuth2TokenGenerator : IAuthTokenGenerator
	{
		private readonly IOAuth2ClientFactory _oAuth2ClientFactory;
		private readonly ITokenProviderFactoryFactory _tokenProviderFactoryFactory;
		private readonly Uri _authenticationUri;
		private readonly ISyncLog _logger;

#pragma warning disable RG0001
		private class OAuth2ClientCredentials : ICredentialsProvider
		{
			private readonly ITokenProvider _tokenProvider;

			public OAuth2ClientCredentials(ITokenProvider tokenProvider)
			{
				_tokenProvider = tokenProvider;
			}

			public NetworkCredential GetCredentials()
			{
				return GetCredentialsAsync().GetAwaiter().GetResult();
			}

			public async Task<NetworkCredential> GetCredentialsAsync(CancellationToken cancellationToken = default)
			{
				string accessToken = await _tokenProvider.GetAccessTokenAsync().ConfigureAwait(false);

				return new NetworkCredential(AuthConstants._RELATIVITY_BEARER_USERNAME, accessToken);
			}
		}
#pragma warning restore RG0001

		public OAuth2TokenGenerator(IOAuth2ClientFactory oAuth2ClientFactory, ITokenProviderFactoryFactory tokenProviderFactoryFactory, Uri authenticationUri, ISyncLog logger)
		{
			_oAuth2ClientFactory = oAuth2ClientFactory;
			_tokenProviderFactoryFactory = tokenProviderFactoryFactory;
			_authenticationUri = authenticationUri;
			_logger = logger;
		}

		public async Task<string> GetAuthTokenAsync(int userId)
		{
			try
			{
				Relativity.Services.Security.Models.OAuth2Client oauth2Client = await _oAuth2ClientFactory.GetOauth2ClientAsync(userId).ConfigureAwait(false);
				ITokenProvider tokenProvider = CreateTokenProvider(oauth2Client);
				string token = await tokenProvider.GetAccessTokenAsync().ConfigureAwait(false);
				return token;
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, $"Failed to get Authentication Token for user with ID: {userId}. Details: {exception.Message}");
				throw;
			}
		}

		private ITokenProvider CreateTokenProvider(Services.Security.Models.OAuth2Client client)
		{
			ITokenProviderFactory providerFactory = _tokenProviderFactoryFactory.Create(GetRelativityStsUri(), client.Id, client.Secret);
			ITokenProvider tokenProvider = providerFactory.GetTokenProvider("WebApi", new List<string> { "UserInfoAccess" });

			// REL-398505: Import API ignores IRelativityTokenProvider when it performs re login and requires below to be set.
			RelativityWebApiCredentialsProvider.Instance().SetProvider(new OAuth2ClientCredentials(tokenProvider));

			return tokenProvider;
		}

		private Uri GetRelativityStsUri()
		{
			string relativityInstance = _authenticationUri.ToString();
			var relativityStsUri = new Uri(Path.Combine(relativityInstance, AuthConstants._RELATIVITY_AUTH_ENDPOINT));
			return relativityStsUri;
		}
	}
}