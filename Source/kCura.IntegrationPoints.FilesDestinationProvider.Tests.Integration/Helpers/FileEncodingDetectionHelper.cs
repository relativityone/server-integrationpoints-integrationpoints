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
			// *** Use Default of Encoding.Default (Ansi CodePage)
			Encoding enc = Encoding.Default;

			// *** Detect byte order mark if any - otherwise assume default
			byte[] buffer = new byte[10];
			FileStream file = new FileStream(srcFile, FileMode.Open);
			file.Read(buffer, 0, 10);
			file.Close();
			
			if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
			{
				enc = Encoding.UTF8;
			}
			else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
			{
				enc = Encoding.Unicode;
			}
			else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xFE && buffer[3] == 0xFF)
			{
				enc = Encoding.UTF32;
			}
			else if (buffer[0] == 0x2B && buffer[1] == 0x2F && buffer[2] == 0x76)
			{
				enc = Encoding.UTF7;
			}
			else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
			{
				// 1201 unicodeFFFE Unicode (Big-Endian)
				enc = Encoding.GetEncoding(1201);
			}
			else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
			{
				// 1200 utf-16 Unicode
				enc = Encoding.GetEncoding(1200);
			}

			return enc;
		}
	}

}
