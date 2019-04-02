namespace Relativity.Sync.Configuration
{
	internal interface IValidationConfiguration : IConfiguration
	{
		string JobName { get; }
		string NotificationEmails { get; }

		string TypeIdentifier { get; }
		string SourceProviderIdentifier { get; }

		int SourceWorkspaceArtifactId { get; }
		int DestinationWorkspaceArtifactId { get; }

		int SavedSearchArtifactId { get; }
		int DestinationFolderArtifactId { get; }

		string FieldsMap { get; }
		int FolderPathSourceFieldArtifactId { get; }
	}
}