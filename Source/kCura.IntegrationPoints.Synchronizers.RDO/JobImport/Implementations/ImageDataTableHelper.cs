using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
	public static class ImageDataTableHelper
	{
		public static DataTable GetDataTable(IDataReader sourceData)
		{
			DataTable rv = new DataTable();
			int fieldCount = sourceData.FieldCount;

			for (int i = 0; i < fieldCount; i++)
			{
				rv.Columns.Add(sourceData.GetName(i));
			}

			while (sourceData.Read())
			{
				DataRow row = rv.NewRow();
				for (int i = 0; i < fieldCount; i++)
				{
					row[rv.Columns[i].ColumnName] = sourceData.GetString(i);
				}
				rv.Rows.Add(row);
			}

			return rv;
		}
	}
}
