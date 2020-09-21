using Relativity.Sync.Storage;
using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface IDocumentDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		IList<FieldMap> GetFieldMappings();
	}
}
