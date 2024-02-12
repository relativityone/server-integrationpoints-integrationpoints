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
            int exitCode = (int)ExitCodes.OK;

            object parsedArgs = null;
            Parser.Default
                .ParseArguments<GenerateDefaultSettingsFileArguments, LoadFromSettingsFileArguments>(args)
                .WithParsed(parsedArguments => { parsedArgs = parsedArguments; });

            if (parsedArgs is GenerateDefaultSettingsFileArguments generateDefaultSettings)
            {
                new GeneratorSettings()
                    .SetDefaultSettings()
                    .ToJsonFile(generateDefaultSettings.OutputSettingsFile);

                Console.WriteLine($"Default settings saved to file: {generateDefaultSettings.OutputSettingsFile}");
            }
            else if (parsedArgs is LoadFromSettingsFileArguments loadFromSettingsFile)
            {
                GeneratorSettings generatorSettings = GeneratorSettings.FromJsonFile(loadFromSettingsFile.InputSettingsFile, loadFromSettingsFile.AppendToWorkspace);

                WorkspaceGeneratorRunner workspaceGeneratorRunner = new WorkspaceGeneratorRunner(generatorSettings);

                exitCode = await workspaceGeneratorRunner.RunAsync().ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("Please provide valid command line arguments.");

                exitCode = (int)ExitCodes.InvalidCommandLineArgs;
            }

            return exitCode;
        }
    }
}
