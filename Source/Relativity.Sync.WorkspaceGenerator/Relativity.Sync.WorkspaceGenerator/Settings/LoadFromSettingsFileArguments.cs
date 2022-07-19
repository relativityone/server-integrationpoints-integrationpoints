using CommandLine;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
    [Verb("inputSettings", HelpText = "Loads settings from specified file")]
    public class LoadFromSettingsFileArguments
    {
        [Option("file", Required = true, HelpText = "Path to configuration file to be used as configuration source instead of command line args.")]
        public string InputSettingsFile { get; set; }

        [Option("appendToWorkspace", Required = false, HelpText = "Flag determining if an existing workspace should be appended with new documents.")]
        public bool AppendToWorkspace { get; set; }
    }
}