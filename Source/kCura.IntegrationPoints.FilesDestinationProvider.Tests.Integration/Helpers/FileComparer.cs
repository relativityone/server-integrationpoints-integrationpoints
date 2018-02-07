using System.IO;
using kCura.Utility;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public static class FileComparer
	{
		public static bool Compare(FileInfo file1, FileInfo file2)
		{
			var hash1 = MD5.GenerateHashForStream(file1.OpenRead());
			var hash2 = MD5.GenerateHashForStream(file2.OpenRead());

			return hash1 == hash2;
		}
	}
}