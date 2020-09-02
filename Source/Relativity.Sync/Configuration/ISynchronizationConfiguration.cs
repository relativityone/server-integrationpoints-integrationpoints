using System;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		char MultiValueDelimiter { get; }

		char NestedValueDelimiter { get; }

		int DestinationWorkspaceArtifactId { get; }

		int DestinationFolderArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		Guid ExportRunId { get; }

		int JobHistoryArtifactId { get; }

		int SourceJobTagArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int SourceWorkspaceTagArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

		bool MoveExistingDocuments { get; }

		int RdoArtifactTypeId { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		FieldOverlayBehavior FieldOverlayBehavior { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		int IdentityFieldId { get; set; }

		string FolderPathSourceFieldName { get; set; }

		string FileSizeColumn { get; set; }

		string NativeFilePathSourceFieldName { get; set; }

		string FileNameColumn { get; set; }

		string OiFileTypeColumnName { get; set; }

		string SupportedByViewerColumn { get; set; }

		bool ImageImport { get; }

		bool IncludeOriginalImages { get; }

		ImportImageFileCopyMode ImportImageFileCopyMode { get; }

		int[] ProductionImagePrecedence { get; }
	}
}