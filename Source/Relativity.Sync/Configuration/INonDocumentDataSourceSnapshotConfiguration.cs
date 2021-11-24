﻿using Relativity.Sync.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface INonDocumentDataSourceSnapshotConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DataSourceArtifactId { get; }

		int RdoArtifactTypeId { get; }

		IList<FieldMap> GetFieldMappings();

		bool IsSnapshotCreated { get; }

		Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount);

		Task SetObjectLinkingSnapshotDataAsync(Guid objectLinkingSnapshotId);
	}
}