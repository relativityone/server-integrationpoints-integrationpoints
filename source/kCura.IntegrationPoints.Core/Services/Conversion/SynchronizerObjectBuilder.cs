﻿using System.Collections.Generic;
using System.Linq;
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

		public T BuildObject<T>(System.Data.IDataRecord row, IEnumerable<string> columns)
		{
			IDictionary<FieldEntry, object> returnValue = new Dictionary<FieldEntry, object>();
			var colList = columns.ToList();
			for (int i = 0; i < row.FieldCount; i++)
			{
				//I made this firstOrDefault and checked for null because the dataset could contain columns not expected
				//I noticed this threw errors if the data set didn't exactly match so I figured we'd skip them
				//if the dev decided to be lazy.
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
