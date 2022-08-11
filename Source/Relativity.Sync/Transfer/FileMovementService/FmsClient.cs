using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly.Retry;
using Relativity.Logging.Tools;
using Relativity.Sync.HttpClient;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <inheritdoc />
    public class FmsClient : IFmsClient
    {
        private string apiV1UrlPart = @"api/v1/DataFactory";
        private readonly IFmsInstanceSettingsService _fmsInstanceSettingsService;
        private readonly ISharedServiceHttpClientFactory _httpClientFactory;
        private readonly IHttpClientRetryPolicyProvider _retryPolicyProvider;

        public FmsClient(
            IFmsInstanceSettingsService ifmsInstanceSettingsService,
            ISharedServiceHttpClientFactory httpClientFactory,
            IHttpClientRetryPolicyProvider retryPolicyProvider)
        {
            _fmsInstanceSettingsService = ifmsInstanceSettingsService ?? throw new ArgumentNullException(nameof(ifmsInstanceSettingsService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));
        }

        public async Task<RunStatusResponse> GetRunStatusAsync(RunStatusRequest runStatusRequest, CancellationToken cancellationToken)
        {
            string getStatusUrl = await GetEndpointUrlAsync("GetStatus").ConfigureAwait(false);

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .GetAsync($"{getStatusUrl}/{runStatusRequest.RunId}?traceId={runStatusRequest.TraceId}", cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                string responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<RunStatusResponse>(responseJson);
            }
        }

        public async Task<CopyListOfFilesResponse> CopyListOfFilesAsync(
            CopyListOfFilesRequest copyListOfFilesRequest, CancellationToken cancellationToken)
        {
            string copyListOfFilesEndpoint = await GetEndpointUrlAsync("CopyListOfFiles").ConfigureAwait(false);

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                string requestJson = JsonConvert.SerializeObject(copyListOfFilesRequest);
                StringContent requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .PostAsync($"{copyListOfFilesEndpoint}", requestContent, cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                string responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<CopyListOfFilesResponse>(responseJson);
            }
        }

        public async Task<string> CancelRunAsync(RunCancelRequest runCancelRequest, CancellationToken cancellationToken)
        {
            string cancelEndpoint = await GetEndpointUrlAsync("Cancel").ConfigureAwait(false);

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                string requestJson = JsonConvert.SerializeObject(runCancelRequest);
                StringContent requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .PostAsync($"{cancelEndpoint}", requestContent, cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private async Task<string> GetEndpointUrlAsync(string method)
        {
            string kubernetesServicesUrl = await _fmsInstanceSettingsService.GetKubernetesServicesUrl();
            string fileMovementServiceUrl = await _fmsInstanceSettingsService.GetFileMovementServiceUrl();

            return $"{kubernetesServicesUrl}/{fileMovementServiceUrl}/{apiV1UrlPart}/{method}";
        }

        private Task<HttpResponseMessage> ExecuteUnderRetryPolicy(Func<Task<HttpResponseMessage>> action)
        {
            RetryPolicy<HttpResponseMessage> retryPolicy = _retryPolicyProvider.GetPolicy();
            Guard.AgainstNull(retryPolicy, nameof(retryPolicy));

            return retryPolicy.ExecuteAsync(action);
        }
    }
}
