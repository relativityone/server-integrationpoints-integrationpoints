﻿namespace kCura.IntegrationPoints.Contracts.Models
{
	public class ExportSettings
	{
		public int SourceWorkspaceArtifactId { set; get; }
		public int TargetWorkspaceArtifactId { get; set; }
	}

	public class ExportUsingSavedSearchSettings : ExportSettings
	{
		public int SavedSearchArtifactId { set; get; }
	}
}