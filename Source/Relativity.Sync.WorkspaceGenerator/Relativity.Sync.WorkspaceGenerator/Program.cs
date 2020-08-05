using System;
using System.Collections.Generic;
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
					RelativityUri = new Uri("https://host.name/Relativity"),
					RelativityServicesUri = new Uri("https://host.name/Relativity/Relativity.Services"),
					RelativityUserName = "user.name",
					RelativityPassword = "passwd",
					DesiredWorkspaceName = "My Test Workspace",
					TemplateWorkspaceName = "Functional Tests Template",
					TestDataDirectoryPath = @"C:\Data\WorkspaceGenerator",
					BatchSize = 10000,
					TestCases = new List<TestCase>()
					{
						new TestCase()
						{
							Name = "TC1",
							NumberOfDocuments = 10,
							NumberOfFields = 15,
							TotalExtractedTextSizeInMB = 5,
							TotalNativesSizeInMB = 8
						}
					}
				};
				defaultSettings.ToJsonFile(generateDefaultSettings.OutputSettingsFile);
				Console.WriteLine($"Default settings saved to file: {generateDefaultSettings.OutputSettingsFile}");
				return (int) ExitCodes.OK;

			}
			else if (settings is LoadFromSettingsFileOptions loadSettingsFromFileOptions)
			{
				generatorSettings = GeneratorSettings.FromJsonFile(loadSettingsFromFileOptions.InputSettingsFile);
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
