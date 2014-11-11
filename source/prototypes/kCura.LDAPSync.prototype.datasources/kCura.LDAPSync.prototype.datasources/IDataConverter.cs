using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	/// <summary>
	/// Provides a means for moving data from a datasource to the destination source
	/// </summary>
	public interface IDataConverter
	{
		/// <summary>
		/// Syncs the data to a destination source
		/// </summary>
		/// <param name="data">The data to be synced</param>
		/// <param name="fieldMap">The list of fields that are expected to be mapped</param>
		void SyncData(IEnumerable<DataRow> data, IEnumerable<FieldMap> fieldMap);
	}
}
