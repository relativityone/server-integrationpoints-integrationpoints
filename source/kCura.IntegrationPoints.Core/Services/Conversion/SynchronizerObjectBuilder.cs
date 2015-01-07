using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public class SynchronizerObjectBuilder : IObjectBuilder
	{
		private List<FieldEntry> _fields;
		public SynchronizerObjectBuilder(List<FieldEntry> fields)
		{
			_fields = fields;
		}

		public T BuildObject<T>(System.Data.IDataRecord row, DataColumnCollection columns)
		{
			IDictionary<FieldEntry, object> returnValue = new Dictionary<FieldEntry, object>();
			for (int i = 0; i < row.FieldCount; i++)
			{
				returnValue.Add(_fields.First(x => x.FieldIdentifier == columns[i].ColumnName), row[i]);
			}
			return (T)returnValue;
		}
	}
}
