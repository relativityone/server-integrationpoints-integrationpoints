namespace Relativity.Sync.Configuration
{
	internal interface IFieldConfiguration
	{
		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; } 
		string FolderPathSourceFieldName { get; }
		int SourceWorkspaceArtifactId { get; }
	}
}