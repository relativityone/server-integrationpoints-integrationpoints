using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Configuration
{
	internal interface IDataSourceSnapshotConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DataSourceArtifactId { get; }

		IList<FieldMap> FieldMappings { get; }
		//MetadataMapping MetadataMapping { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		int FolderPathSourceFieldArtifactId { get; }

		bool IsSnapshotCreated { get; }

		Task SetSnapshotDataAsync(Guid runId, long totalRecordsCount);
	}
}