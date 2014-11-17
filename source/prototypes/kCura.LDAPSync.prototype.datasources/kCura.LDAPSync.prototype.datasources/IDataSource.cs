using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	public interface IDataSourceProvider
	{
		/// <summary>
		/// Gets the data from the source
		/// </summary>
		/// <param name="entries">List of field Entries that are expected to be mapped</param>
		/// <param name="entryIds">List of field Entries that are expected to be mapped</param>
		/// <returns>A datareader that allows for a datasource to be read</returns>
		IDataReader GetData(IEnumerable<FieldEntry> entries, IEnumerable<string> entryIds, string options);

		IDataReader GetBatchableData(FieldEntry identifier, string options);

	}
}
