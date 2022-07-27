using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public class SynchronizerObjectBuilder : IObjectBuilder
	{
		private readonly Dictionary<string, FieldEntry> _fieldsDictionary;
		public SynchronizerObjectBuilder(IEnumerable<FieldEntry> fields)
		{
			_fieldsDictionary = fields.ToDictionary(k => k.FieldIdentifier, v => v);
		}

		public T BuildObject<T>(System.Data.IDataRecord row)
		{
			IDictionary<FieldEntry, object> returnValue = new Dictionary<FieldEntry, object>();

			for (int i = 0; i < row.FieldCount; i++)
			{
				if(_fieldsDictionary.TryGetValue(row.GetName(i), out FieldEntry field))
				{
					returnValue.Add(field, row[i]);
				}
			}

			return (T)returnValue;
		}
	}
}
