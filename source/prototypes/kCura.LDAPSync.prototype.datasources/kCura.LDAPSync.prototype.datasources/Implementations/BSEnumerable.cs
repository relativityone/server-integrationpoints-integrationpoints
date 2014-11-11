using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	/// <summary>
	/// Intermediate source could be used for buffering, filtering, etc..
	/// </summary>
	public class BsEnumerable : IEnumerable<DataRow>
	{
		private readonly DataTable _source;
		public BsEnumerable(DataTable source)
		{
			_source = source;
		}
		public IEnumerator<DataRow> GetEnumerator()
		{
			foreach (DataRow row in _source.Rows)
			{
				if (row.Field<string>(0).Contains("skip"))
				{
					continue;
				}
				yield return row;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
