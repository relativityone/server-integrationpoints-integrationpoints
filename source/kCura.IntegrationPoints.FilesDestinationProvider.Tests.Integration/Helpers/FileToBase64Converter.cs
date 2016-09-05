using System.IO;
using System.Text;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public static class FileToBase64Converter
	{
		public static string Convert(string filePath)
		{
			var fileInfo = new FileInfo(filePath);

			byte[] bytes = File.ReadAllBytes(fileInfo.FullName);
			return System.Convert.ToBase64String(bytes);
		}
	}
}