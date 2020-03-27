using System;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class GeneratorSettingsReader
	{
		public GeneratorSettings ReadFromConsole()
		{
			GeneratorSettings settings = new GeneratorSettings
			{
				RelativityUri = ReadUri(nameof(GeneratorSettings.RelativityUri)),
				RelativityUserName = ReadString(nameof(GeneratorSettings.RelativityUserName)),
				RelativityPassword = ReadString(nameof(GeneratorSettings.RelativityPassword)),
				TemplateWorkspaceName = ReadString(nameof(GeneratorSettings.TemplateWorkspaceName)),
				TestDataDirectoryPath = ReadString(nameof(GeneratorSettings.TestDataDirectoryPath)),
				DesiredWorkspaceName = ReadString(nameof(GeneratorSettings.DesiredWorkspaceName)),
				NumberOfDocuments = ReadInt(nameof(GeneratorSettings.NumberOfDocuments)),
				NumberOfFields = ReadInt(nameof(GeneratorSettings.NumberOfFields)),
				TotalNativesSizeInMB = ReadInt(nameof(GeneratorSettings.TotalNativesSizeInMB)),
				TotalExtractedTextSizeInMB = ReadInt(nameof(GeneratorSettings.TotalExtractedTextSizeInMB))
			};

			return settings;
		}

		public GeneratorSettings DefaultSettings()
		{
			return new GeneratorSettings()
			{
				RelativityUri = new Uri("https://tvmdev/Relativity"),
				RelativityUserName = "relativity.admin@kcura.com",
				RelativityPassword = "Test1234!",
				TemplateWorkspaceName = "Functional Tests Template",
				DesiredWorkspaceName = "Test",
				TestDataDirectoryPath = @"C:\Data\WorkspaceGenerator",
				NumberOfDocuments = 20,
				NumberOfFields = 10,
				TotalNativesSizeInMB = 10,
				TotalExtractedTextSizeInMB = 5
			};
		}

		private Uri ReadUri(string paramName)
		{
			Uri value = new Uri("http://initial");
			bool isParsed = false;

			while (!isParsed)
			{
				Console.WriteLine($"{paramName}:");
				isParsed = Uri.TryCreate(Console.ReadLine(), UriKind.Absolute, out value);
			}

			return value;
		}

		private int ReadInt(string paramName)
		{
			int value = 0;
			bool isParsed = false;

			while (!isParsed)
			{
				Console.WriteLine($"{paramName}:");
				isParsed = int.TryParse(Console.ReadLine(), out value);
			}

			return value;
		}

		private string ReadString(string paramName)
		{
			Console.WriteLine($"{paramName}:");
			return Console.ReadLine();
		}
	}
}