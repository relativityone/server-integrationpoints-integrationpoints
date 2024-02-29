using CommandLine;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
    [Verb("generateDefaultSettings", HelpText = "Generates default settings file")]
    public class GenerateDefaultSettingsFileArguments
    {
        [Option("file", Required = true, HelpText = "JSON file name or path")]
        public string OutputSettingsFile { get; set; }
    }
}