using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class EnumerableExtensions
	{
		public static DataTable ToDataTable(this IEnumerable<int> ints)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add();

			foreach (var i in ints)
			{
				var dataRow = dataTable.NewRow();
				dataRow[0] = i;
				dataTable.Rows.Add(dataRow);
			}

			return dataTable;
		}
	}
}