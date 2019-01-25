using System.IO;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal static class FileEncodingDetectionHelper
	{
		private const short _BUFFER_SIZE = 4;

		/// <summary>
		/// Method that performs simple check of the file encoding based on BOM 
		/// </summary>
		/// <param name="srcFile">file name under encoding investigation</param>
		/// <returns></returns>
		public static Encoding GetFileEncoding(string srcFile)
		{
			byte[] buffer = new byte[_BUFFER_SIZE];
			using (FileStream file = new FileStream(srcFile, FileMode.Open))
			{
				file.Read(buffer, 0, _BUFFER_SIZE);
				file.Close();
			}

			if (IsUTF8(buffer))
			{
				return Encoding.UTF8;
			}
			if (IsUnicode(buffer))
			{
				return Encoding.Unicode;
			}
			if (IsBigEndiancUnicode(buffer))
			{
				return Encoding.BigEndianUnicode;
			}

			return Encoding.Default;
		}

		private static bool IsUTF8(byte[] buffer)
		{
			const short requiredBufferLength = 4;

			ValidateBufferLength(requiredBufferLength, buffer);

			return buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF;
		}
		private static bool IsUnicode(byte[] buffer)
		{
			const short requiredBufferLength = 2;

			ValidateBufferLength(requiredBufferLength, buffer);

			return buffer[0] == 0xFF && buffer[1] == 0xFE;
		}

		private static bool IsBigEndiancUnicode(byte[] buffer)
		{
			const short requiredBufferLength = 2;

			ValidateBufferLength(requiredBufferLength, buffer);

			return buffer[0] == 0xFE && buffer[1] == 0xFF;
		}

		private static void ValidateBufferLength(short requiredBufferLength, byte[] buffer)
		{
			if (buffer == null || buffer.Length < requiredBufferLength)
			{
				throw new TestException($"Buffer should have required {requiredBufferLength} bytes length or more!");
			}
		}

	}

}
