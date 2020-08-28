using Relativity.Sync.Configuration;
using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	internal sealed class DocumentDataSourceSnapshotConfiguration : DataSourceSnapshotConfigurationBase,
		IDocumentDataSourceSnapshotConfiguration, IDocumentRetryDataSourceSnapshotConfiguration
	{
		private readonly IFieldMappings _fieldMappings;

		public DocumentDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
			: base(cache, syncJobParameters)
		{
			_fieldMappings = fieldMappings;
		}

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();
	}
}
