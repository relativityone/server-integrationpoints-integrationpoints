﻿using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface IDataSourceSnapshotConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DataSourceArtifactId { get; }

		/// <summary>
		///     Change from string to list of objects
		/// </summary>
		string FieldMappings { get; }

		bool IsSnapshotCreated { get; }

		Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount);
	}
}