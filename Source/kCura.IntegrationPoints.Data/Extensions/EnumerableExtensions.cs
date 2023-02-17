using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using kCura.Data;
using DataRow = System.Data.DataRow;

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
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = i;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public static kCura.Data.DataView ToDataView(this DataSet dataSet)
        {
            return new kCura.Data.DataView(dataSet);
        }

        public static kCura.Data.DataView ToDataView<T>(this IEnumerable<T> collection) where T : class
        {
            DataSet dataSet = collection.ToDataSet();
            return new kCura.Data.DataView(dataSet);
        }

        public static DataSet ToDataSet<T>(this IEnumerable<T> collection) where T : class
        {
            var dataSet = new DataSet();
            DataTable dataTable = collection.ToDataTable();
            dataSet.Tables.Add(dataTable);
            return dataSet;
        }

        /// <summary>
        /// Creates DataTable object from model containing simple type properties
        /// </summary>
        /// <typeparam name="T">Model with simple type properties</typeparam>
        /// <param name="collection">Collection of models</param>
        /// <returns>Converted DataTable object</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection) where T : class
        {
            DataTable dataTable = new DataTable();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            CreateColumns(properties, dataTable);
            PopulateRows(collection, dataTable, properties);

            return dataTable;
        }

        private static void PopulateRows<T>(
            IEnumerable<T> collection,
            DataTable dataTable,
            PropertyInfo[] properties)
                where T : class
        {
            foreach (T item in collection)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow.BeginEdit();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(item, index: null) != null)
                    {
                        dataRow[property.Name] = property.GetValue(item, index: null);
                    }
                }

                dataRow.EndEdit();
                dataTable.Rows.Add(dataRow);
            }
        }

        private static void CreateColumns(PropertyInfo[] properties, DataTable dataTable)
        {
            foreach (PropertyInfo property in properties)
            {
                Type columnType = property.PropertyType;
                if (columnType.IsGenericType)
                {
                    columnType = columnType.GetGenericArguments()[0];
                }

                dataTable.Columns.Add(property.Name, columnType);
            }
        }
    }
}
