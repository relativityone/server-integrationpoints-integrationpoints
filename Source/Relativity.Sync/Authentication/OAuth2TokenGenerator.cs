using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Relativity.OAuth2Client.Interfaces;

namespace Relativity.Sync.Authentication
{
	internal sealed class OAuth2TokenGenerator : IAuthTokenGenerator
	{
		private const string _RELATIVITY_AUTH_ENDPOINT = "Identity/connect/token";
		private readonly IOAuth2ClientFactory _oAuth2ClientFactory;
		private readonly ITokenProviderFactoryFactory _tokenProviderFactoryFactory;
		private readonly Uri _authenticationUri;
		private readonly ISyncLog _logger;

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
			return tokenProvider;
		}

		private Uri GetRelativityStsUri()
		{
			string relativityInstance = _authenticationUri.ToString();
			var relativityStsUri = new Uri(Path.Combine(relativityInstance, _RELATIVITY_AUTH_ENDPOINT));
			return relativityStsUri;
		}
	}
}