namespace Relativity.Sync.Executors
{
	internal sealed class RelativitySourceCaseTag
	{
		public int ArtifactId { get; set; }
		
		public string Name { get; set; }

		public int SourceWorkspaceArtifactId { get; set; }

		public string SourceWorkspaceName { get; set; }

		public string SourceInstanceName { get; set; }
	}
}
