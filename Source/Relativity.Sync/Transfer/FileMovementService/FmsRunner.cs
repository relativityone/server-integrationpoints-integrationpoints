using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Transfer.FileMovementService.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <inheritdoc />
    internal class FmsRunner : IFmsRunner
    {
        internal const string SubmittedStatusName = "Submitted";
        private static readonly string[] _activeBatchStatuses =
        {
            SubmittedStatusName,
            RunStatuses.Queued,
            RunStatuses.InProgress,
            RunStatuses.Canceling,
        };

        private readonly IFmsClient _fmsClient;
        private readonly IFmsInstanceSettingsService _fmsInstanceSettingsService;
        private readonly IAPILog _logger;

        private string _endpointUrlBase;

        public FmsRunner(IFmsClient fmsClient, IFmsInstanceSettingsService fmsInstanceSettingsService, IAPILog logger)
        {
            _fmsClient = fmsClient;
            _fmsInstanceSettingsService = fmsInstanceSettingsService;
            _logger = logger;
        }

        public async Task<List<FmsBatchStatusInfo>> RunAsync(List<FmsBatchInfo> batches, CancellationToken cancellationToken)
        {
            Guid batchesTraceId = batches.FirstOrDefault()?.TraceId ?? Guid.Empty;
            try
            {
                _logger.LogInformation("Starting transfer of {count} fms batches. TraceId: {traceId}", batches.Count, batchesTraceId);

                string endpointUrl = await GetFullEndpointUrlAsync(FmsConstants.CopyListOfFilesMethodName).ConfigureAwait(false);
                IEnumerable<CopyListOfFilesRequest> requests = batches
                    .Select(batch => new CopyListOfFilesRequest
                    {
                        TraceId = batch.TraceId,
                        DestinationPath = batch.DestinationLocationShortPath,
                        SourcePath = batch.SourceLocationShortPath,
                        PathToListOfFiles = batch.UploadedBatchFilePath,
                        EndpointURL = endpointUrl
                    });

                List<Task<CopyListOfFilesResponse>> tasks = requests
                    .Select(r => _fmsClient.CopyListOfFilesAsync(r, cancellationToken))
                    .ToList();

                CopyListOfFilesResponse[] responses = await Task.WhenAll(tasks).ConfigureAwait(false);

                _logger.LogInformation("Transfer of {count} fms batches started. TraceId: {traceId}", batches.Count, batchesTraceId);

                return responses.Select(ConvertToStatusInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during running fms batches. TraceId: {traceId}", batchesTraceId);
                throw;
            }
        }

        public async Task<List<FmsBatchStatusInfo>> MonitorAsync(List<FmsBatchStatusInfo> batches, CancellationToken cancellationToken)
        {
            Guid batchesTraceId = batches.FirstOrDefault()?.TraceId ?? Guid.Empty;
            try
            {
                _logger.LogInformation("Start monitoring of {count} fms batches. TraceId: {traceId}", batches, batchesTraceId);

                int intervalSeconds = await _fmsInstanceSettingsService.GetMonitoringInterval().ConfigureAwait(false);
                List<FmsBatchStatusInfo> activeBatches = GetActiveBatches(batches);
                string endpointUrl = await GetFullEndpointUrlAsync(FmsConstants.GetStatusMethodName);

                while (activeBatches.Any())
                {
                    _logger.LogInformation("Found {count} active fms batches. Checking their states. TraceId: {traceId}", activeBatches.Count, batchesTraceId);

                    await UpdateBatchStatusesAsync(activeBatches, endpointUrl, cancellationToken).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken).ConfigureAwait(false);
                    activeBatches = GetActiveBatches(batches);
                }

                _logger.LogInformation("Monitoring of {count} fms batches finished. TraceId: {traceId}", batches.Count, batchesTraceId);
                PrintBatchesStatus(batches);
                return batches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch statuses monitoring. TraceId: {traceId}", batchesTraceId);
                throw;
            }
        }

        private async Task UpdateBatchStatusesAsync(List<FmsBatchStatusInfo> activeBatches, string endpointUrl, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<Task> tasks = activeBatches
                .Select(batch => UpdateSingleBatchAsync(batch, endpointUrl, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateSingleBatchAsync(FmsBatchStatusInfo batch, string endpointUrl, CancellationToken cancellationToken)
        {
            RunStatusRequest request = new RunStatusRequest
            {
                TraceId = batch.TraceId,
                RunId = batch.RunId,
                EndpointURL = endpointUrl
            };

            RunStatusResponse response = await _fmsClient.GetRunStatusAsync(request, cancellationToken).ConfigureAwait(false);
            batch.Status = response.Status;
            batch.StatusMessage = response.Message;
        }

        private static List<FmsBatchStatusInfo> GetActiveBatches(List<FmsBatchStatusInfo> batches)
        {
            return batches
                .Where(b => b.Status.IsIn(StringComparison.InvariantCultureIgnoreCase, _activeBatchStatuses))
                .ToList();
        }

        private static FmsBatchStatusInfo ConvertToStatusInfo(CopyListOfFilesResponse startResponse)
        {
            return new FmsBatchStatusInfo
            {
                TraceId = startResponse.TraceId,
                RunId = startResponse.RunId,
                Status = SubmittedStatusName,
            };
        }

        private void PrintBatchesStatus(List<FmsBatchStatusInfo> batches)
        {
            foreach (FmsBatchStatusInfo batch in batches)
            {
                _logger.LogInformation("FMS Batch RunId: {FmsRunId} Status: {FmsStatus} StatusMessage: {FmsStatusMessage}", batch.RunId, batch.Status, batch.StatusMessage);
            }
        }

        private async Task GetEndpointUrlBaseAsync()
        {
            try
            {
                string kubernetesServicesUrl = await _fmsInstanceSettingsService.GetKubernetesServicesUrl().ConfigureAwait(false);
                string fileMovementServiceUrl = await _fmsInstanceSettingsService.GetFileMovementServiceUrl().ConfigureAwait(false);

                _endpointUrlBase = $"{kubernetesServicesUrl}/{fileMovementServiceUrl}/{FmsConstants.ApiV1UrlPart}";
                _logger.LogInformation("Base endpoint for FMS Batch processing: {endpointUrlBase}", _endpointUrlBase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create endpoint URL base part");
                throw;
            }
        }

        private async Task<string> GetFullEndpointUrlAsync(string method)
        {
            if (string.IsNullOrEmpty(_endpointUrlBase))
            {
                await GetEndpointUrlBaseAsync().ConfigureAwait(false);
            }

            return $"{_endpointUrlBase}/{method}";
        }
    }
}
