using System;
using System.IO;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles
{
	internal static class LoadFilesGenerator
	{
		private const string NATIVES_LOAD_FILE_HEADER = "Control Number,FILE_PATH";

		private static readonly string NATIVES_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesLoadFile.csv");

		public static string GetOrCreateNativesLoadFile()
		{
			if (File.Exists(NATIVES_LOAD_FILE_PATH))
			{
				return NATIVES_LOAD_FILE_PATH;
			}

			using (FileStream nativesLoadFileStream = new FileStream(NATIVES_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter nativesLoadFileWriter = new StreamWriter(nativesLoadFileStream))
				{
					nativesLoadFileWriter.WriteLine(NATIVES_LOAD_FILE_HEADER);

					foreach (var native in Natives.NATIVES)
					{
						nativesLoadFileWriter.WriteLine($"{native.Key},{native.Value}");
					}
				}
			}

			return NATIVES_LOAD_FILE_PATH;
		}
	}
}
