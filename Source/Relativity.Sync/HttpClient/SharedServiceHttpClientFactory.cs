using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Helpers;

namespace Relativity.Sync.HttpClient
{
    public class SharedServiceHttpClientFactory : ISharedServiceHttpClientFactory
    {
        private const string ServiceName = "SharedServices";
        private readonly IHelper _helper;

        public SharedServiceHttpClientFactory(IHelper helper)
        {
            _helper = helper;
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
            using (IAuthTokenProvider authTokenProvider =
                   _helper.GetServicesManager().CreateProxy<IAuthTokenProvider>(ExecutionIdentity.System))
            {
                return await authTokenProvider.GetAuthToken(serviceName).ConfigureAwait(false);
            }
        }
    }
}
