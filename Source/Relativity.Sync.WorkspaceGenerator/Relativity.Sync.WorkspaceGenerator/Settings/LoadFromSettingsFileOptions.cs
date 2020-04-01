using CommandLine;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
	[Verb("inputSettings", HelpText = "Loads settings from specified file")]
	public class LoadFromSettingsFileOptions
	{
		[JsonIgnore]
		[Option("file", Required = true, HelpText = "Path to configuration file to be used as configuration source instead of command line args.")]
		public string InputSettingsFile { get; set; }
	}
}