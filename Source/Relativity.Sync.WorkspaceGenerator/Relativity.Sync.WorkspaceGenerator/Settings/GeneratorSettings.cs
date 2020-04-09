using System;
using System.IO;
using CommandLine;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
	[Verb("cli", HelpText = "Reads settings from command line and runs application")]
	public class GeneratorSettings
	{
		private Uri _relativityUri;
		[Option("relativityUrl", Required = true, HelpText = "Relativity URL e.g. https://host.name/Relativity")]
		public Uri RelativityUri
		{
			get => _relativityUri;
			set
			{
				_relativityUri = value;
				RelativityWebApiUri = new Uri(value, "/RelativityWebAPI");
				RelativityServicesUri = new Uri(value, "/Relativity.Services");
				RelativityRestApiUri = new Uri(value, "/Relativity.Rest/api");
			}
		}

		[Option("userName", Required = true, HelpText = "Relativity user name")]
		public string RelativityUserName { get; set; }

		[Option("password", Required = true, HelpText = "Relativity user password")]
		public string RelativityPassword { get; set; }

		[Option("templateWorkspaceName", HelpText = "Name of the template workspace.", Default = "Functional Tests Template")]
		public string TemplateWorkspaceName { get; set; }

		[Option("workspaceName", Required = true, HelpText = "Name of the workspace to be created.")]
		public string DesiredWorkspaceName { get; set; }

		[Option("testDataDir", Required = true, HelpText = "Directory path where test data (natives and extracted text) will be stored")]
		public string TestDataDirectoryPath { get; set; }

		[Option("numberOfDocuments", Required = true, HelpText = "Desired number of documents")]
		public int NumberOfDocuments { get; set; }

		[Option("numberOfFields", Required = true, HelpText = "Desired number of random fields")]
		public int NumberOfFields { get; set; }

		[Option("nativesSizeInMB", Required = true, HelpText = "Desired total size (in MB) of natives to be generated. Put 0 to disable natives.")]
		public int TotalNativesSizeInMB { get; set; }

		[Option("textSizeInMB", Required = true, HelpText = "Desired total size of extracted texts (in MB) to be generated. Put 0 to disable extracted text")]
		public int TotalExtractedTextSizeInMB { get; set; }

		[JsonIgnore]
		public Uri RelativityWebApiUri { get; private set; }

		[JsonIgnore]
		public Uri RelativityServicesUri { get; private set; }

		[JsonIgnore]
		public Uri RelativityRestApiUri { get; private set; }

		[JsonIgnore]
		public bool GenerateNatives => TotalNativesSizeInMB > 0;

		[JsonIgnore]
		public bool GenerateExtractedText => TotalExtractedTextSizeInMB > 0;

		public void WriteToJsonFile(string filePath)
		{
			File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public static GeneratorSettings FromJsonFile(string filePath)
		{
			return JsonConvert.DeserializeObject<GeneratorSettings>(File.ReadAllText(filePath));
		}
	}
}