using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.WinEDDS.Credentials;
using Relativity.API;
using Relativity.OAuth2Client.Interfaces;
using Relativity.Services.Security.Models;
using ITokenProvider = Relativity.OAuth2Client.Interfaces.ITokenProvider;

namespace kCura.IntegrationPoints.Core.Authentication
{
    public class OAuth2TokenGenerator : IAuthTokenGenerator
    {
        private readonly IAPILog _logger;
        private readonly IOAuth2ClientFactory _oAuth2ClientFactory;
        private readonly ITokenProviderFactoryFactory _tokenProviderFactory;
        private readonly CurrentUser _contextUser;

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

            public async Task<NetworkCredential> GetCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                string accessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

                return new NetworkCredential(AuthConstants._RELATIVITY_BEARER_USERNAME, accessToken);
            }
        }

        public OAuth2TokenGenerator(IHelper helper, IOAuth2ClientFactory oAuth2ClientFactory, ITokenProviderFactoryFactory tokenProviderFactory, CurrentUser contextUser)
        {
            _oAuth2ClientFactory = oAuth2ClientFactory;
            _contextUser = contextUser;
            _tokenProviderFactory = tokenProviderFactory;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<OAuth2TokenGenerator>();
        }

        public string GetAuthToken()
        {
            try
            {
                OAuth2Client oauth2Client = _oAuth2ClientFactory.GetOauth2ClientAsync(_contextUser.ID).GetAwaiter().GetResult();
                ITokenProvider tokenProvider = CreateTokenProvider(oauth2Client);
                string token = tokenProvider.GetAccessTokenAsync().GetAwaiter().GetResult();

                return token;
            }
            catch (Exception exception)
            {
                LogGetAuthTokenError(exception);
                throw;
            }
        }

        private ITokenProvider CreateTokenProvider(OAuth2Client client)
        {
            ITokenProviderFactory providerFactory = _tokenProviderFactory.Create(GetRelativityStsUri(), client.Id, client.Secret);
            ITokenProvider tokenProvider = providerFactory.GetTokenProvider("WebApi", new List<string> { "UserInfoAccess" });
            RelativityWebApiCredentialsProvider.Instance().SetProvider(new OAuth2ClientCredentials(tokenProvider));
            return tokenProvider;
        }

        private Uri GetRelativityStsUri()
        {
            string relativityInstance = ExtensionPointServiceFinder.ServiceUriProvider
                .AuthenticationUri().ToString();
            var relativityStsUri = new Uri(System.IO.Path.Combine(relativityInstance, Constants.IntegrationPoints.RELATIVITY_AUTH_ENDPOINT));            
            return relativityStsUri;
        }        

        private void LogGetAuthTokenError(Exception exception)
        {
            _logger.LogError(exception,
                $"Failed to get Authentication Token for user with ID: {_contextUser}. Details: {exception.Message}");
        }
    }
}