using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

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

		public static DataSet ToDataSet<T>(this IEnumerable<T> collection) where T : class
		{
			return collection.ToDataTable().DataSet;
		}

		public static DataTable ToDataTable<T>(this IEnumerable<T> collection) where T : class
		{
			DataTable dt = new DataTable();
			Type t = typeof(T);
			PropertyInfo[] pia = t.GetProperties();

			//Inspect the properties and create the columns in the DataTable
			foreach (PropertyInfo pi in pia)
			{
				Type columnType = pi.PropertyType;
				if (columnType.IsGenericType)
				{
					columnType = columnType.GetGenericArguments()[0];
				}
				dt.Columns.Add(pi.Name, columnType);
			}

			//Populate the data table
			foreach (T item in collection)
			{
				DataRow dr = dt.NewRow();
				dr.BeginEdit();
				foreach (PropertyInfo pi in pia)
				{
					if (pi.GetValue(item, null) != null)
					{
						dr[pi.Name] = pi.GetValue(item, null);
					}
				}
				dr.EndEdit();
				dt.Rows.Add(dr);
			}
			return dt;
		}
	}
}