using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using kCura.LDAPSync.prototype.datasources.Implementations;

namespace kCura.LDAPSync.prototype.datasources
{
	public class Program
	{
		private static IEnumerable<FieldEntry> GetDataSourceFields()
		{
			var ls = new JsonFieldReader("fields.json");
			var fields = ls.GetFields();
			return fields;
		}

		private static IEnumerable<FieldEntry> GetOutputFields()
		{
			var ls = new JsonFieldReader("destinationFields.json");
			var fields = ls.GetFields();
			return fields;
		}

		public static void Main(string[] args)
		{
			var source = new JsonDataSource("source.json");
			var dt = GetDataSource(source);

			var converter = new FileDataConverter("output.txt");
			var sourceFields = GetDataSourceFields();
			var destinationFields = GetOutputFields();
			IEnumerable<FieldMap> result = sourceFields.Zip(destinationFields, (x, y) => new FieldMap
			{
				SourceField = x,
				DestinationField = y
			});

			converter.SyncData(new BsEnumerable(dt), result);
		}



		private static DataTable GetDataSource(IDataSource source)
		{
			var reader = source.GetData(new List<FieldEntry>());

			var dt = reader.GetSchemaTable();
			while (reader.Read())
			{
				var r = dt.NewRow();
				for (var i = 0; i < dt.Columns.Count; i++)
				{
					r[i] = reader.GetString(i);
				}
				dt.Rows.Add(r);
			}
			return dt;
		}

	}
}
