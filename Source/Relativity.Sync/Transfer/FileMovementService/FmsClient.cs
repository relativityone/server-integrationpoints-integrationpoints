using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Relativity.Logging.Tools;
using Relativity.Sync.HttpClient;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <inheritdoc />
    public class FmsClient : IFmsClient
    {
        private string apiV1UrlPart = @"api/v1/DataFactory";
        private readonly IFmsInstanceSettingsService _iFmsInstanceSettingsService;
        private readonly ISharedServiceHttpClientFactory _httpClientFactory;
        private readonly IHttpClientRetryPolicyProvider _retryPolicyProvider;

        public FmsClient(
            IFmsInstanceSettingsService ifmsInstanceSettingsService,
            ISharedServiceHttpClientFactory httpClientFactory,
            IHttpClientRetryPolicyProvider retryPolicyProvider)
        {
            _iFmsInstanceSettingsService = ifmsInstanceSettingsService ?? throw new ArgumentNullException(nameof(ifmsInstanceSettingsService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));
        }

        public async Task<RunStatusResponse> GetRunStatusAsync(
            RunStatusRequest runStatusRequest, CancellationToken cancellationToken)
        {
            var getStatusUrl = await GetEndpointUrl("GetStatus");

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                var responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .GetAsync($"{getStatusUrl}/{runStatusRequest.RunId}?traceId={runStatusRequest.TraceId}", cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                var responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<RunStatusResponse>(responseJson);
            }
        }

        public async Task<CopyListOfFilesResponse> CopyListOfFilesAsync(
            CopyListOfFilesRequest copyListOfFilesRequest, CancellationToken cancellationToken)
        {
            var copyListOfFilesEndpoint = await GetEndpointUrl("CopyListOfFiles");

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                var requestJson = JsonConvert.SerializeObject(copyListOfFilesRequest);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .PostAsync($"{copyListOfFilesEndpoint}", requestContent, cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                var responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<CopyListOfFilesResponse>(responseJson);
            }
        }

        public async Task<string> CancelRunAsync(RunCancelRequest runCancelRequest, CancellationToken cancellationToken)
        {
            var cancelEndpoint = await GetEndpointUrl("Cancel");

            using (var httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
            {
                var requestJson = JsonConvert.SerializeObject(runCancelRequest);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                    .PostAsync($"{cancelEndpoint}", requestContent, cancellationToken)
                    .ConfigureAwait(false));

                responseMessage.EnsureSuccessStatusCode();

                return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private async Task<string> GetEndpointUrl(string method)
        {
            var kubernetesServicesUrl = await _iFmsInstanceSettingsService.GetKubernetesServicesUrl();
            var fileMovementServiceUrl = await _iFmsInstanceSettingsService.GetFileMovementServiceUrl();

            return $"{kubernetesServicesUrl}/{fileMovementServiceUrl}/{apiV1UrlPart}/{method}";
        }

        private Task<HttpResponseMessage> ExecuteUnderRetryPolicy(Func<Task<HttpResponseMessage>> action)
        {
            var retryPolicy = _retryPolicyProvider.GetPolicy();
            Guard.AgainstNull(retryPolicy, nameof(retryPolicy));

            return retryPolicy.ExecuteAsync(action);
        }
    }
}
