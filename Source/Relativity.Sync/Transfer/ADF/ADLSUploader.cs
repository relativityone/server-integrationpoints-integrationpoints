using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADF
{
    internal class AdlsUploader : IADLSUploader
    {
        private readonly IHelperWrapper _helper;
        private readonly IAPILog _logger;

        public AdlsUploader(IHelperWrapper helperWrapper, IAPILog logger)
        {
            _helper = helperWrapper;
            _logger = logger.ForContext<AdlsUploader>();
        }

        public string CreateBatchFile(Dictionary<int, FilePathInfo> locationsDictionary, CancellationToken cancellationToken)
        {
            if (locationsDictionary == null || locationsDictionary.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            string loadFileHeader = "Source,Destination";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(loadFileHeader);
            foreach (FilePathInfo location in locationsDictionary.Values)
            {
                stringBuilder.AppendLine($"{location.SourceLocationShortToLoadFile},{location.DestinationLocationFullPathToLink}");

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("ADLS Batch file Generation cancelled");
                    return string.Empty;
                }
            }

            string loadFilePath = Path.Combine(Path.GetTempPath(), "ADLSBatchFile_" + Guid.NewGuid());
            File.WriteAllText(loadFilePath, stringBuilder.ToString());

            return loadFilePath;
        }

        public async Task<string> UploadFileAsync(string sourceFilePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentNullException(nameof(sourceFilePath), "Source file path is null or empty.");
            }

            const int maxNumberOfRetries = 3;

            void OnRetryAction(Exception ex, TimeSpan waitTime, int retryCount, Context context)
            {
                _logger.LogWarning(ex, "Encountered issue while loading file to ADLS, attempting to retry. Retry count: {retryCount} Wait time: {waitTimeMs} (ms)", retryCount, waitTime.TotalMilliseconds);
            }

            string destinationFilePath;
            async Task<string> ExecutionFunction()
            {
                IStorageAccess<string> storageAccess = await _helper.GetStorageAccessorAsync(cancellationToken).ConfigureAwait(false);

                string destinationDir = await GetADLSDestinationDirectory(cancellationToken);
                destinationFilePath = Path.Combine(destinationDir, $"BatchFile{Guid.NewGuid()}.csv");
                _logger.LogInformation("ADLS Batch file Path - {destinationFilePath}", destinationFilePath);

                CopyFileOptions copyFileOptions = new CopyFileOptions
                {
                    DestinationParentDirectoryNotExistsBehavior = DirectoryNotExistsBehavior.CreateIfNotExists,
                    DestinationExistsBehavior = FileExistsBehavior.OverwriteIfExists
                };
                await storageAccess.CreateDirectoryAsync(destinationDir, cancellationToken: cancellationToken).ConfigureAwait(false);
                await storageAccess.CopyFileAsync(sourceFilePath, destinationFilePath, copyFileOptions, cancellationToken).ConfigureAwait(false);
                return destinationFilePath;
            }

            string OnExceptionFunction(Exception exception)
            {
                _logger.LogError(exception.Message);
                throw exception;
            }

            string OnCancellationFunction(Exception exception)
            {
                _logger.LogWarning("ADLS Batch file upload cancelled.");
                return string.Empty;
            }

            destinationFilePath = await RetryPolicyRunAsync(
                maxNumberOfRetries,
                OnRetryAction,
                ExecutionFunction,
                OnExceptionFunction,
                OnCancellationFunction,
                cancellationToken)
                .ConfigureAwait(false);
            return destinationFilePath;
        }

        private async Task<string> GetADLSDestinationDirectory(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            StorageEndpoint[] storages = await _helper.GetStorageEndpointsAsync(cancellationToken).ConfigureAwait(false);
            string destinationDir;
            if (storages != null && storages.Length > 0)
            {
                StorageEndpoint storageEndpoint = storages.First();
                destinationDir = Path.Combine(
                    @"\\",
                    storageEndpoint.EndpointFqdn,
                    storageEndpoint.PrimaryStorageContainer,
                    "Files",
                    "RIP_BatchFiles");
            }
            else
            {
                string message = "Storage Endpoint not found for the tenant. Please ensure that tenant is fully migrated to ADLS";
                _logger.LogError(message);
                throw new NotFoundException(message);
            }

            return destinationDir;
        }

        private async Task<T> RetryPolicyRunAsync<T>(int maxNumberOfRetries, Action<Exception, TimeSpan, int, Context> onRetryAction, Func<Task<T>> executionFunction, Func<Exception, T> onExceptionFunction, Func<Exception, T> onCancellationFunction, CancellationToken cancellationToken) where T : class
        {
            const int maxJitterMs = 100;
            const int betweenRetriesBase = 2;

            RetryPolicy policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    maxNumberOfRetries,
                    retryAttempt =>
                    {
                        TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(betweenRetriesBase, retryAttempt));
                        TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, maxJitterMs));
                        return delay + jitter;
                    },
                    onRetryAction);
            Context policyContext = new Context("RetryContext");
            policyContext.Add("CancellationToken", cancellationToken);
            PolicyResult<T> result = await policy.ExecuteAndCaptureAsync((ctx, ct) => executionFunction(), policyContext, cancellationToken)
                .ConfigureAwait(false);

            Exception exception = result.FinalException;
            if (cancellationToken.IsCancellationRequested)
            {
                return onCancellationFunction(exception);
            }

            if (exception != null)
            {
                return onExceptionFunction(exception);
            }

            return result.Result;
        }

        
    }
}
