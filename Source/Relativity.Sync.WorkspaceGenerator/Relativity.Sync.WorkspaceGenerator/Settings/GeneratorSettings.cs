using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
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
				RelativityRestApiUri = new Uri(value, "/Relativity.Rest/api");
			}
		}

		[Option("relativityServicesUrl", Required = true, HelpText = "Relativity Services URL e.g. https://host.name/Relativity/Relativity.Services")]
		public Uri RelativityServicesUri { get; set; }

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

		[Option("batchSize", HelpText = "Size of batch for documents import", Default = 10000)]
		public int BatchSize { get; set; }

		public List<TestCase> TestCases { get; set; } = new List<TestCase>();

		[JsonIgnore]
		public Uri RelativityWebApiUri { get; private set; }

		[JsonIgnore]
		public Uri RelativityRestApiUri { get; private set; }

		public void ToJsonFile(string filePath)
		{
			File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public static GeneratorSettings FromJsonFile(string filePath)
		{
			return JsonConvert.DeserializeObject<GeneratorSettings>(File.ReadAllText(filePath));
		}
	}
}