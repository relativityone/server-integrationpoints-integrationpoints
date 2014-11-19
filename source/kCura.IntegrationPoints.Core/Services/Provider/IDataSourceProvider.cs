using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public interface IDataSourceProvider : IFieldProvider
	{

		IDataReader GetData(IEnumerable<FieldEntry> entries, IEnumerable<string> entryIds, string options);

		IDataReader GetBatchableIds(FieldEntry identifier, string options);

	}
}
