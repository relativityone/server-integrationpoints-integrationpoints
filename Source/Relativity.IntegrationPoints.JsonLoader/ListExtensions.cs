using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Relativity.IntegrationPoints.JsonLoader
{
	public static class ListExtensions
	{
		public static DataTable ToBatchableIds<T>(this List<T> items, string columnName)
		{	
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(columnName, typeof(string));
				
			PropertyInfo property = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).First(prop => prop.Name == columnName);

			foreach (T item in items)
			{
				object value = property.GetValue(item, null);
				dataTable.Rows.Add(value);
			}

			return dataTable;
		}

		public static DataTable ToDataTable<T>(this List<T> items, string identifier, IEnumerable<string> fieldList, HashSet<string> entryIds)
		{
			DataTable dataTable = new DataTable(typeof(T).Name);

			PropertyInfo identifierProperty = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).First(prop => prop.Name == identifier);
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => fieldList.Contains(prop.Name)).ToArray();

			foreach (PropertyInfo prop in properties)
			{
				dataTable.Columns.Add(prop.Name, prop.PropertyType);
			}

			foreach (T item in items)
			{
				string value = (string) identifierProperty.GetValue(item, null);
				if (entryIds.Contains(value))
				{
					object[] values = new object[properties.Length];
					for (int i = 0; i < properties.Length; i++)
					{
						values[i] = properties[i].GetValue(item, null);
					}
					dataTable.Rows.Add(values);
				}
			}
			return dataTable;
		}
	}
}
