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
			Encoding enc = Encoding.Default;

			byte[] buffer = new byte[4];
			FileStream file = new FileStream(srcFile, FileMode.Open);
			file.Read(buffer, 0, 4);
			file.Close();
			
			if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
			{
				enc = Encoding.UTF8;
			}
			else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
			{
				enc = Encoding.Unicode;
			}
			else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
			{
				enc = Encoding.BigEndianUnicode;
			}

			return enc;
		}
	}

}
