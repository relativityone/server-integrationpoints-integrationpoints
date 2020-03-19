using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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
			return new AzureStorageHelper(TestSettingsConfig.AzureStorageConnection, TestSettingsConfig.AzureStoragePerformanceContainer);
		}

		public string DownloadFile(string filePath, string destinationPath, bool copySubfolders)
		{
			CloudBlockBlob blob = _container.GetBlockBlobReference(filePath);
			if(blob == null)
			{
				throw new FileNotFoundException();
			}

			string outputFile = copySubfolders
				? Path.Combine(destinationPath, Path.GetFileName(filePath))
				: Path.Combine(destinationPath, filePath);

			blob.DownloadToFile(outputFile, FileMode.OpenOrCreate);

			return outputFile;
		}
	}
}
