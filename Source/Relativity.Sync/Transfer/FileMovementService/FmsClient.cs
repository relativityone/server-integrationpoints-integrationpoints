using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;
using Relativity.API;
using Relativity.Logging.Tools;
using Relativity.Sync.HttpClient;
using Relativity.Sync.Transfer.FileMovementService.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <inheritdoc />
    internal class FmsClient : IFmsClient
    {
        private readonly ISharedServiceHttpClientFactory _httpClientFactory;
        private readonly IHttpClientRetryPolicyProvider _retryPolicyProvider;
        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        public FmsClient(
            ISharedServiceHttpClientFactory httpClientFactory,
            IHttpClientRetryPolicyProvider retryPolicyProvider,
            ISerializer serializer,
            IAPILog logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RunStatusResponse> GetRunStatusAsync(RunStatusRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Checking run status of Run ID: {runId} Trace ID: {traceId}", request.RunId, request.TraceId);

                using (System.Net.Http.HttpClient httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
                {
                    HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () =>
                    {
                        string requestUri = $"{request.EndpointURL}/{request.RunId}?traceId={request.TraceId}";

                        return await httpClient
                            .GetAsync(requestUri, cancellationToken)
                            .ConfigureAwait(false);
                    });

                    responseMessage.EnsureSuccessStatusCode();

                    string responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return _serializer.Deserialize<RunStatusResponse>(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check run status");
                throw;
            }
        }

        public async Task<CopyListOfFilesResponse> CopyListOfFilesAsync(CopyListOfFilesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Copying list of files. Trace ID: {traceId}", request.TraceId);

                using (System.Net.Http.HttpClient httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
                {
                    string requestJson = _serializer.Serialize(request);
                    _logger.LogInformation("FMS Request {request}", requestJson);
                    StringContent requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                        .PostAsync($"{request.EndpointURL}", requestContent, cancellationToken)
                        .ConfigureAwait(false));

                    responseMessage.EnsureSuccessStatusCode();

                    string responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return _serializer.Deserialize<CopyListOfFilesResponse>(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy list of files");
                throw;
            }
        }

        public async Task<string> CancelRunAsync(RunCancelRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Cancelling Run ID: {runId} Trace ID: {traceId}", request.RunId, request.TraceId);

                using (System.Net.Http.HttpClient httpClient = await _httpClientFactory.GetHttpClientAsync().ConfigureAwait(false))
                {
                    string requestJson = _serializer.Serialize(request);
                    StringContent requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage responseMessage = await ExecuteUnderRetryPolicy(async () => await httpClient
                        .PostAsync($"{request.EndpointURL}", requestContent, cancellationToken)
                        .ConfigureAwait(false));

                    responseMessage.EnsureSuccessStatusCode();

                    return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel run");
                throw;
            }
        }

        private Task<HttpResponseMessage> ExecuteUnderRetryPolicy(Func<Task<HttpResponseMessage>> action)
        {
            RetryPolicy<HttpResponseMessage> retryPolicy = _retryPolicyProvider.GetPolicy();
            Guard.AgainstNull(retryPolicy, nameof(retryPolicy));

            return retryPolicy.ExecuteAsync(action);
        }
    }
}
