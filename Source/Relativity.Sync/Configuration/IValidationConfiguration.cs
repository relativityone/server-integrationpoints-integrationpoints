using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Configuration
{
	internal interface IValidationConfiguration : IConfiguration
	{
		string JobName { get; }
		string NotificationEmails { get; }
		int SourceWorkspaceArtifactId { get; }
		int DestinationWorkspaceArtifactId { get; }
		int SavedSearchArtifactId { get; }
		int DestinationFolderArtifactId { get; }
		string FieldMappings { get; }
		int FolderPathSourceFieldArtifactId { get; }
		ImportOverwriteMode ImportOverwriteMode { get; }
		FieldOverlayBehavior FieldOverlayBehavior { get; }
		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }
	}
}