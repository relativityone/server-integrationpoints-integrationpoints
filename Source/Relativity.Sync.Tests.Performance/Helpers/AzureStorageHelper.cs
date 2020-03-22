using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Relativity.Sync.Tests.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class AzureStorageHelper
	{
		private readonly CloudBlobContainer _container;

		public AzureStorageHelper(string storageConnection, string containerName)
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
			CloudBlobClient client = storageAccount.CreateCloudBlobClient();

			_container = client.GetContainerReference(containerName);
		}

		public static AzureStorageHelper CreateFromTestConfig()
		{
			return new AzureStorageHelper(AppSettings.AzureStorageConnection, AppSettings.AzureStoragePerformanceContainer);
		}

		public async Task<string> DownloadFileAsync(string filePath, string destinationPath)
		{
			CloudBlockBlob blob = _container.GetBlockBlobReference(filePath);
			if(blob == null)
			{
				throw new FileNotFoundException();
			}

			string outputFile = Path.Combine(destinationPath, Path.GetFileName(filePath));

			await blob.DownloadToFileAsync(outputFile, FileMode.OpenOrCreate).ConfigureAwait(false);

			return outputFile;
		}
	}
}
