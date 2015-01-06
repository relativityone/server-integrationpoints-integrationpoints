using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	public interface IDataSourceProvider : IFieldProvider
	{
		IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options);
		IDataReader GetBatchableIds(FieldEntry identifier, string options);
	}
}
