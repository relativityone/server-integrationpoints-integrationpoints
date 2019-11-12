using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public class SynchronizerObjectBuilder : IObjectBuilder
	{
		private readonly IEnumerable<FieldEntry> _fields;
		public SynchronizerObjectBuilder(IEnumerable<FieldEntry> fields)
		{
			_fields = fields;
		}

		public T BuildObject<T>(System.Data.IDataRecord row, IEnumerable<string> columns)
		{
			IDictionary<FieldEntry, object> returnValue = new Dictionary<FieldEntry, object>();
			List<string> colList = columns.ToList();
			for (int i = 0; i < row.FieldCount; i++)
			{
				var fieldName = _fields.FirstOrDefault(x => x.FieldIdentifier == colList[i]);
				if (fieldName != null)
				{
					returnValue.Add(fieldName, row[i]);	
				}
			}
			return (T)returnValue;
		}
	}
}
