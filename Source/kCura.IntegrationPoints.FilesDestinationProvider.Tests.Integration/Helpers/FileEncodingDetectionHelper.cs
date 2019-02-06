using System.IO;
using System.Text;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal static class FileEncodingDetectionHelper
	{
		/// <summary>
		/// Method that performs simple check of the file encoding based on BOM 
		/// </summary>
		/// <param name="srcFile">file name under encoding investigation</param>
		/// <returns></returns>
		public static Encoding GetFileEncoding(string srcFile)
		{
			Encoding encoding;
			using (var reader = new StreamReader(srcFile, Encoding.Default, true))
			{
				int _ = reader.Peek(); // required to trigger automatic encoding detection
				encoding = reader.CurrentEncoding;
			}

			return encoding;
		}
		
	}

}
