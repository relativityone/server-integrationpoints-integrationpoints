using System.Collections.Generic;
using System.Data;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
	public static class ReaderUtil
	{
		public static IList<IDictionary<string, object>> Read(IDataReader reader)
		{
			DataColumnCollection columns = reader.GetSchemaTable().Columns;

			IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
			while (reader.Read())
			{
				IDictionary<string, object> row = new Dictionary<string, object>();

				for (int i = 0; i < columns.Count; ++i)
				{
					row[columns[i].ColumnName] = reader.GetValue(i);
				}

				result.Add(row);
			}

			return result;
		}
	}
}
