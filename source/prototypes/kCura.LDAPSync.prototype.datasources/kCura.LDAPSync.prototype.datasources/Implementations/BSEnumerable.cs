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
	public class BsEnumerable : IEnumerable<IDictionary<FieldEntry, object>>
	{
		private readonly DataTable _source;
		private readonly IDictionary<string,FieldEntry> _entries; 
		public BsEnumerable(DataTable source, IEnumerable<FieldEntry> entries)
		{
			_source = source;
			_entries = entries.ToDictionary(x => x.FieldIdentifier, x=>x);
		}

		public IEnumerator<IDictionary<FieldEntry, object>> GetEnumerator()
		{
			foreach (DataRow row in _source.Rows)
			{
				var dictionary = new Dictionary<FieldEntry, object>();
				foreach (DataColumn column in row.Table.Columns)
				{
					dictionary.Add(_entries[column.ColumnName], row[column]);
				}
				yield return dictionary;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
