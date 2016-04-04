using System;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public class SourceConfiguration
	{
		public int SavedSearchArtifactId { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public int TargetWorkspaceArtifactId { get; set; }
	}
}