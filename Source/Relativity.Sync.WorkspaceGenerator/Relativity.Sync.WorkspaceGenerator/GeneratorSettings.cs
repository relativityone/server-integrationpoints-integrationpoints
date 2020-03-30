using System;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class GeneratorSettings
	{
		private Uri _relativityUri;
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

		public Uri RelativityWebApiUri { get; private set; }
		public Uri RelativityServicesUri { get; private set; }
		public Uri RelativityRestApiUri { get; private set; }

		public string RelativityUserName { get; set; }
		public string RelativityPassword { get; set; }
		public string TemplateWorkspaceName { get; set; }
		public string DesiredWorkspaceName { get; set; }
		public string TestDataDirectoryPath { get; set; }
		public int NumberOfDocuments { get; set; }
		public int NumberOfFields { get; set; }
		public int TotalNativesSizeInMB { get; set; }
		public int TotalExtractedTextSizeInMB { get; set; }

		public bool GenerateNatives => TotalNativesSizeInMB > 0;
		public bool GenerateExtractedText => TotalExtractedTextSizeInMB > 0;
	}
}