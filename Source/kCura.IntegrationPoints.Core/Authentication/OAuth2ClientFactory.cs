using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Authentication
{
    public class OAuth2ClientFactory : IOAuth2ClientFactory
    {
        private readonly IRetryHandler _retryHandler;
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public OAuth2ClientFactory(IRetryHandlerFactory retryHandlerFactory, IHelper helper)
        {
            const int maxNumberOfRetries = 4;
            _retryHandler = retryHandlerFactory.Create(maxNumberOfRetries);
            _helper = helper;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>();
        }

        public async Task<OAuth2Client> GetOauth2ClientAsync(int userId)
        {
            string clientName = GenerateClientName(userId);

            try
            {
                OAuth2Client client;
                using (var oAuth2ClientManager = _helper.GetServicesManager().CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System))
                {
                    client = await ReadOAuth2ClientByNameAsyncWithRetries(oAuth2ClientManager, clientName)
                        .ConfigureAwait(false);

                    if (client == null)
                    {
                        client = await CreateNewOAuth2ClientAsyncWithRetries(oAuth2ClientManager, clientName, userId)
                            .ConfigureAwait(false);
                    }
                }

                LogGetOAuth2ClientSuccess(userId);
                return client;
            }
            catch (Exception ex)
            {
                LogGetOAuth2ClientError(ex);
                throw new InvalidOperationException($"Failed to retrieve OAuth2Client for user with id: {userId}", ex);
            }
        }

        private async Task<OAuth2Client> ReadOAuth2ClientByNameAsyncWithRetries(IOAuth2ClientManager oAuth2ClientManager, string clientName)
        {
            try
            {
                return await _retryHandler.ExecuteWithRetriesAsync(
                    async () =>
                    {
                        IEnumerable<OAuth2Client> clients = await oAuth2ClientManager.ReadAllAsync()
                            .ConfigureAwait(false);
                        return clients.SingleOrDefault(x => x.Name.Equals(clientName));
                    }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(IOAuth2ClientManager)} failed on {nameof(IOAuth2ClientManager.ReadAllAsync)} with {ex.Message}");
                throw;
            }
        }

        private async Task<OAuth2Client> CreateNewOAuth2ClientAsyncWithRetries(IOAuth2ClientManager oAuth2ClientManager, string clientName, int userId)
        {
            try
            {
                return await _retryHandler.ExecuteWithRetriesAsync(
                    () => oAuth2ClientManager.CreateAsync(
                            clientName,
                            OAuth2Flow.ClientCredentials,
                            new List<Uri>(),
                            userId)
                ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(IOAuth2ClientManager)} failed on {nameof(IOAuth2ClientManager.CreateAsync)} with {ex.Message}");
                throw;
            }
        }

        private string GenerateClientName(int userId)
        {
            return $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {userId}";
        }

        private void LogGetOAuth2ClientError(Exception ex)
        {
            _logger.LogError(ex, "Error occured while getting OAuth2Client: {errorMessage}", ex.Message);
        }

        private void LogGetOAuth2ClientSuccess(int userId)
        {
            _logger.LogInformation("OAuth2Client for user with id: {userId} created successfully", userId);
        }
    }
}
