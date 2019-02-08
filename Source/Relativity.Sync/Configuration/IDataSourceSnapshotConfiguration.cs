using System;

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

		void SetSnapshotData(Guid runId, int totalRecordsCount);
	}
}