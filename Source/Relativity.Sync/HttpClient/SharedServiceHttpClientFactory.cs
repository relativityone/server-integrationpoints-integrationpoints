using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Helpers;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.HttpClient
{
    internal class SharedServiceHttpClientFactory : ISharedServiceHttpClientFactory
    {
        private const string ServiceName = "SharedServices";
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IAPILog _logger;

        public SharedServiceHttpClientFactory(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _logger = logger;
        }

        public async Task<System.Net.Http.HttpClient> GetHttpClientAsync()
        {
            try
            {
                string token = await GetCidTokenAsync(ServiceName).ConfigureAwait(false);

                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpClient.Timeout = TimeSpan.FromMinutes(2); // 120s is a default time described by Auth Team
                return httpClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create HttpClient for service name: {serviceName}", ServiceName);
                throw;
            }
        }

        private async Task<string> GetCidTokenAsync(string serviceName)
        {
            try
            {
                using (IAuthTokenProvider authTokenProvider = await _serviceFactoryForAdmin.CreateProxyAsync<IAuthTokenProvider>().ConfigureAwait(false))
                {
                    return await authTokenProvider.GetAuthToken(serviceName).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get auth token for service name: {serviceName}", serviceName);
                throw;
            }
        }
    }
}
