using System;
using System.Threading.Tasks;
using CommandLine;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			Console.Title = "Workspace Generator";

			object settings = null;
			Parser
				.Default
				.ParseArguments<GenerateDefaultSettingsFile, LoadFromSettingsFileOptions, GeneratorSettings>(args)
				.WithParsed(parsedSettings => { settings = parsedSettings; });

			GeneratorSettings generatorSettings = null;

			if (settings is GenerateDefaultSettingsFile generateDefaultSettings)
			{
				GeneratorSettings defaultSettings = new GeneratorSettings()
				{
					RelativityUri = new Uri("http://example.uri"),
					RelativityUserName = "user.name",
					RelativityPassword = "passwd",
					DesiredWorkspaceName = "My Test Workspace",
					TemplateWorkspaceName = "Functional Tests Template",
					TestDataDirectoryPath = @"C:\Data\WorkspaceGenerator",
					NumberOfDocuments = 100,
					NumberOfFields = 15,
					TotalNativesSizeInMB = 20,
					TotalExtractedTextSizeInMB = 10
				};
				defaultSettings.WriteToJsonFile(generateDefaultSettings.OutputSettingsFile);
				Console.WriteLine($"Default settings saved to file: {generateDefaultSettings.OutputSettingsFile}");
				return (int) ExitCodes.OK;

			}
			else if (settings is LoadFromSettingsFileOptions loadSettingsFromFileOptions)
			{
				generatorSettings = GeneratorSettings.FromJsonFile(loadSettingsFromFileOptions.InputSettingsFile);
			}
			else if (settings is GeneratorSettings)
			{
				generatorSettings = settings as GeneratorSettings;
			}
			else
			{
				Console.WriteLine("Please provide valid command line arguments.");
				return (int)ExitCodes.InvalidCommandLineArgs;
			}

			WorkspaceGeneratorRunner workspaceGeneratorRunner = new WorkspaceGeneratorRunner(generatorSettings);
			return await workspaceGeneratorRunner.RunAsync().ConfigureAwait(false);
		}
	}
}
