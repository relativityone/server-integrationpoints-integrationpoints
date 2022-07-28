using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADF
{
    internal class ADLSUploader : IADLSUploader
    {
        private readonly IHelperWrapper _helper;
        private readonly IAPILog _logger;

        public ADLSUploader(IHelperWrapper helperWrapper, IAPILog logger)
        {
            _helper = helperWrapper;
            _logger = logger.ForContext<ADLSUploader>();
        }

        public string CreateBatchFile(Dictionary<int, FilePathInfo> locationsDictionary, CancellationToken cancellationToken)
        {
            if (locationsDictionary == null || locationsDictionary.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return String.Empty;
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
                    return String.Empty;
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

            IStorageAccess<string> storageAccess = await _helper.GetStorageAccessorAsync(cancellationToken).ConfigureAwait(false);

            string destinationDir = await GetADLSDestinationDirectory(cancellationToken);
            string destinationFilePath = Path.Combine(destinationDir, $"BatchFile{Guid.NewGuid()}.csv");
            _logger.LogInformation("ADLS Batch file Path - {destinationFilePath}", destinationFilePath);
            try
            {
                CopyFileOptions copyFileOptions = new CopyFileOptions
                {
                    DestinationParentDirectoryNotExistsBehavior = DirectoryNotExistsBehavior.CreateIfNotExists,
                    DestinationExistsBehavior = FileExistsBehavior.OverwriteIfExists
                };
                await storageAccess.CreateDirectoryAsync(destinationDir, cancellationToken: cancellationToken).ConfigureAwait(false);
                await storageAccess.CopyFileAsync(sourceFilePath, destinationFilePath, copyFileOptions, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("ADLS Batch file upload cancelled.");
                    return String.Empty;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            
            return destinationFilePath;
        }

        private async Task<string> GetADLSDestinationDirectory(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return String.Empty;
            }

            StorageEndpoint[] storages = await _helper.GetStorageEndpointsAsync(cancellationToken).ConfigureAwait(false);
            string destinationDir;
            if (storages != null && storages.Length > 0)
            {
                StorageEndpoint storageEndpoint = storages.First();
                destinationDir = Path.Combine(@"\\",
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

        private async Task RetryPolicyRunAsync(int maxNumberOfRetries, Action<Exception, TimeSpan, int, Context> onRetryAction, Func<Task> executionFunction)
        {
            const int maxJitterMs = 100;
            const int betweenRetriesBase = 2;

            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxNumberOfRetries, retryAttempt =>
                    {
                        TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(betweenRetriesBase, retryAttempt));
                        TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, maxJitterMs));
                        return delay + jitter;
                    },
                    onRetryAction)
                .ExecuteAsync(executionFunction)
                .ConfigureAwait(false);
        }


    }
}
