using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		bool ImageImport { get; }

		int RdoArtifactTypeId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int DestinationFolderArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		Guid ExportRunId { get; }

		int JobHistoryArtifactId { get; }

		int SourceJobTagArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int SourceWorkspaceTagArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

		int IdentityFieldId { get; set; }

		string FileNameColumn { get; set; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		FieldOverlayBehavior FieldOverlayBehavior { get; }

		string FolderPathSourceFieldName { get; set; }

		bool MoveExistingDocuments { get; }

		int DataSourceArtifactId { get; }

		bool EnableTagging { get; }

		Task<int> GetImportApiBatchSizeAsync();
	}
}
