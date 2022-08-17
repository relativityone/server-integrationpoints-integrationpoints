using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Helpers;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.HttpClient
{
    internal class SharedServiceHttpClientFactory : ISharedServiceHttpClientFactory
    {
        private const string ServiceName = "SharedServices";
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;

        public SharedServiceHttpClientFactory(ISourceServiceFactoryForAdmin serviceFactoryForAdmin)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
        }

        public async Task<System.Net.Http.HttpClient> GetHttpClientAsync()
        {
            string token = await GetCidTokenAsync(ServiceName).ConfigureAwait(false);

            System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.Timeout = TimeSpan.FromMinutes(2); // 120s is a default time described by Auth Team
            return httpClient;
        }

        private async Task<string> GetCidTokenAsync(string serviceName)
        {
            using (IAuthTokenProvider authTokenProvider = await _serviceFactoryForAdmin.CreateProxyAsync<IAuthTokenProvider>().ConfigureAwait(false))
            {
                return await authTokenProvider.GetAuthToken(serviceName).ConfigureAwait(false);
            }
        }
    }
}
