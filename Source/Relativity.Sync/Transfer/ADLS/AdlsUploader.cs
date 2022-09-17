using System;
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

namespace Relativity.Sync.Transfer.ADLS
{
    internal class AdlsUploader : IAdlsUploader
    {
        private const int _MAX_NUMBER_OF_RETRIES = 3;
        private const int _MAX_JITTER_MS = 100;

        private readonly IHelperWrapper _helper;
        private readonly IAPILog _logger;

        private double _betweenRetriesBase = 2;

        public AdlsUploader(IHelperWrapper helperWrapper, IAPILog logger)
        {
            _helper = helperWrapper;
            _logger = logger.ForContext<AdlsUploader>();
        }

        public string CreateBatchFile(FmsBatchInfo storedLocation, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            string loadFileHeader = "Source,Destination";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(loadFileHeader);

            foreach (var file in storedLocation.Files)
            {
                string sourcePath = file.FileName;
                string destinationPath = Path.Combine(storedLocation.DestinationLocationShortPath, file.FileName);
                stringBuilder.AppendLine($"{sourcePath},{destinationPath}");

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("ADLS Batch file Generation cancelled");
                    return string.Empty;
                }
            }

            string batchFilePath = Path.Combine(Path.GetTempPath(), "ADLSBatchFile_" + Guid.NewGuid());
            File.WriteAllText(batchFilePath, stringBuilder.ToString());

            return batchFilePath;
        }

        public async Task<string> UploadFileAsync(string sourceFilePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentNullException(nameof(sourceFilePath), "Source file path is null or empty.");
            }

            void OnRetryAction(Exception ex, TimeSpan waitTime, int retryCount, Context context)
            {
                _logger.LogWarning(ex, "Encountered issue while loading file to ADLS, attempting to retry. Retry count: {retryCount} Wait time: {waitTimeMs} (ms)", retryCount, waitTime.TotalMilliseconds);
            }

            string destinationFilePath;
            async Task<string> ExecutionFunction()
            {
                IStorageAccess<string> storageAccess = await _helper.GetStorageAccessorAsync(cancellationToken).ConfigureAwait(false);
                string batchFileName = $"BatchFile_{Guid.NewGuid()}.csv";

                string destinationDir = await GetAdlsDestinationDirectory(cancellationToken);
                destinationFilePath = Path.Combine(destinationDir, batchFileName);
                _logger.LogInformation("ADLS Batch file Path - {destinationFilePath}", destinationFilePath);

                CopyFileOptions copyFileOptions = new CopyFileOptions
                {
                    DestinationParentDirectoryNotExistsBehavior = DirectoryNotExistsBehavior.CreateIfNotExists,
                    DestinationExistsBehavior = FileExistsBehavior.OverwriteIfExists
                };
                await storageAccess.CreateDirectoryAsync(destinationDir, cancellationToken: cancellationToken).ConfigureAwait(false);
                await storageAccess.CopyFileAsync(sourceFilePath, destinationFilePath, copyFileOptions, cancellationToken).ConfigureAwait(false);
                string batchLocationBasedOnAdls = GetAdlsRelativeLocation(batchFileName);

                return batchLocationBasedOnAdls;
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
                OnRetryAction,
                ExecutionFunction,
                OnExceptionFunction,
                OnCancellationFunction,
                cancellationToken)
                .ConfigureAwait(false);
            return destinationFilePath;
        }

        public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken)
        {
            IStorageAccess<string> storageAccess = await _helper.GetStorageAccessorAsync(cancellationToken).ConfigureAwait(false);
            DeleteFileOptions deleteFileOptions = new DeleteFileOptions
            {
                FileNotExistsBehavior = FileNotExistsBehavior.ReturnResultObjIfNotExists,
                Force = true
            };

            _logger.LogInformation("Deleting ADLS batch file - {filePath}", filePath);
            DeleteFileResult deleteResultObject = await storageAccess.DeleteFileAsync(filePath, deleteFileOptions, cancellationToken).ConfigureAwait(false);
            if (deleteResultObject == DeleteFileResult.FileNotFound)
            {
                _logger.LogWarning("Unable to delete file, because it was not found - {filePath}", filePath);
            }

            if (deleteResultObject != DeleteFileResult.Success && cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("ADLS file deletion cancelled, file path - {filePath}", filePath);
            }
        }

        private string GetAdlsRelativeLocation(string fileName)
        {
            string relativeLocation = Path.Combine("Temp", "RIP_BatchFiles", fileName);
            return relativeLocation.Replace('\\', '/');
        }

        private async Task<string> GetAdlsDestinationDirectory(CancellationToken cancellationToken)
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
                    "Temp",
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

        private async Task<T> RetryPolicyRunAsync<T>(Action<Exception, TimeSpan, int, Context> onRetryAction, Func<Task<T>> executionFunction, Func<Exception, T> onExceptionFunction, Func<Exception, T> onCancellationFunction, CancellationToken cancellationToken) where T : class
        {
            RetryPolicy policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _MAX_NUMBER_OF_RETRIES,
                    retryAttempt =>
                    {
                        TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(_betweenRetriesBase, retryAttempt));
                        TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, _MAX_JITTER_MS));
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
