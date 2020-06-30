using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface IValidationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int SavedSearchArtifactId { get; }

		int DestinationFolderArtifactId { get; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		FieldOverlayBehavior FieldOverlayBehavior { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		int? JobHistoryToRetryId { get; }

		string GetJobName();

		string GetNotificationEmails();

		IList<FieldMap> GetFieldMappings();

		string GetFolderPathSourceFieldName();
	}
}