using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	public interface IDataSource
	{
		/// <summary>
		/// Gets the data from the
		/// </summary>
		/// <param name="entries">List of field Entries that are expected to be mapped</param>
		/// <returns>A datareader that allows for a datasource to be read</returns>
		IDataReader GetData(IEnumerable<FieldEntry> entries);
	}
}
