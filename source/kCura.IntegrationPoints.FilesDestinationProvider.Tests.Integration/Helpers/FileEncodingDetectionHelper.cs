using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			byte[] buffer = new byte[4];
			using (FileStream file = new FileStream(srcFile, FileMode.Open))
			{
				file.Read(buffer, 0, 4);
			}
			if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
				return Encoding.UTF8;
			if (buffer[0] == 0xFF && buffer[1] == 0xFE)
				return Encoding.Unicode;
			if (buffer[0] == 0xFE && buffer[1] == 0xFF)
				return Encoding.BigEndianUnicode;
			if (buffer[0] == 0xFE && buffer[1] == 0x43)
				return Encoding.Default;

			throw new Exception($"Not supported encoding type found in file: {srcFile}");
		}
	}

}
