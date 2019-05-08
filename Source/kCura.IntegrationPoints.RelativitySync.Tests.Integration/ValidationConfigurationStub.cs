using System.Collections.Generic;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class ValidationConfigurationStub : IValidationConfiguration
	{
		public string JobName { get; set; }
		public string NotificationEmails { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public int DestinationFolderArtifactId { get; set; }
		public IList<FieldMap> FieldMappings { get; }
		public int FolderPathSourceFieldArtifactId { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
	}
}