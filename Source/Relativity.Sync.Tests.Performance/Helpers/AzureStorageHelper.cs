using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Performance.Helpers
{
    public class AzureStorageHelper
    {
        private readonly CloudBlobContainer _container;

        public AzureStorageHelper(string connectionString, string containerName)
        {
            CloudStorageAccount.UseV1MD5 = true;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            _container = client.GetContainerReference(containerName);
        }

        public static AzureStorageHelper CreateFromTestConfig()
        {
            return new AzureStorageHelper(AppSettings.AzureStorageConnectionString, AppSettings.AzureStoragePerformanceContainer);
        }

        public async Task<string> DownloadFileAsync(string filePath, string destinationPath)
        {
            CloudBlockBlob blob = _container.GetBlockBlobReference(filePath);
            if (blob == null)
            {
                throw new FileNotFoundException();
            }

            string outputFile = Path.Combine(destinationPath, Path.GetFileName(filePath));
            await blob.DownloadToFileAsync(outputFile, FileMode.Create).ConfigureAwait(false);

            return outputFile;
        }
    }
}
