using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public interface IDataSourceProvider : IFieldProvider
	{
		IDataReader GetData(IEnumerable<FieldEntry> entries, IEnumerable<string> entryIds, string options);
		IDataReader GetBatchableIds(FieldEntry identifier, string options);
	}
}
