using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	/// <summary>
	/// Provides a means to query a datasource to see which fields are available to map
	/// </summary>
	public interface IFieldProvider
	{
		/// <summary>
		/// Gets the fields that the datasource deems mapable
		/// </summary>
		/// <returns>A list of field Entries that can be mapped</returns>
		IEnumerable<FieldEntry> GetFields();
	}
}
