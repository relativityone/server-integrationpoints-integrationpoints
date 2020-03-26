using System;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class GeneratorSettings
	{
		public Uri RelativityUri { get; set; }
		public string RelativityUserName { get; set; }
		public string RelativityPassword { get; set; }
		public string TemplateWorkspaceName { get; set; }
		public string DesiredWorkspaceName { get; set; }
		public int NumberOfDocuments { get; set; }
		public int NumberOfFixedLengthTextFields { get; set; }
		public int TotalNativesSizeInMB { get; set; }
	}
}