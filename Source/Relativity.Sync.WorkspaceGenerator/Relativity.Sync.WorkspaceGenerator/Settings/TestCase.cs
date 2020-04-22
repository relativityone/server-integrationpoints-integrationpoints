using System.Collections.Generic;
using CommandLine;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
	public class TestCase
	{
		[Option("name", Required = true, HelpText = "Name of the test case")]
		public string Name { get; set; }

		[Option("numberOfDocuments", Required = true, HelpText = "Desired number of documents")]
		public int NumberOfDocuments { get; set; }

		[Option("numberOfFields", Required = true, HelpText = "Desired number of random fields")]
		public int NumberOfFields { get; set; }

		[Option("nativesSizeInMB", Required = true, HelpText = "Desired total size (in MB) of natives to be generated. Put 0 to disable natives.")]
		public int TotalNativesSizeInMB { get; set; }

		[Option("textSizeInMB", Required = true, HelpText = "Desired total size of extracted texts (in MB) to be generated. Put 0 to disable extracted text")]
		public int TotalExtractedTextSizeInMB { get; set; }

		[JsonIgnore]
		public bool GenerateNatives => TotalNativesSizeInMB > 0;

		[JsonIgnore]
		public bool GenerateExtractedText => TotalExtractedTextSizeInMB > 0;

		[JsonIgnore]
		public List<CustomField> Fields { get; set; } = new List<CustomField>();
	}
}