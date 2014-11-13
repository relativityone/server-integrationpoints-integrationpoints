using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources.Extensions
{
	public static class DataTableExtensions
	{
		public static void LoadReader(this DataTable table, IDataReader reader)
		{
			var dt = reader.GetSchemaTable();
			foreach (DataColumn column in dt.Columns)
			{
				table.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
			}
			while (reader.Read())
			{
				var r = table.NewRow();
				for (var i = 0; i < table.Columns.Count; i++)
				{
					r[i] = reader.GetString(i);
				}
				table.Rows.Add(r);
			}
		}

		public static DataTable GetDataFromReader(this IDataReader reader)
		{
			var dt = new DataTable();
			dt.LoadReader(reader);
			return dt;
		}
	}
}
