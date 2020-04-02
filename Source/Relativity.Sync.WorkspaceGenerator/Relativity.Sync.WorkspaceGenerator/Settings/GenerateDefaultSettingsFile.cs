using CommandLine;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
	[Verb("generateDefaultSettings", HelpText = "Generates default settings file")]
	public class GenerateDefaultSettingsFile
	{
		[JsonIgnore]
		[Option("file", Required = true, HelpText = "JSON file name or path")]
		public string OutputSettingsFile { get; set; }
	}
}