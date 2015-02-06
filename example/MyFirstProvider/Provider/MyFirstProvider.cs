using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace Provider
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider(GlobalConstants.FIRST_PROVIDER_GUID)]
	public class MyFirstProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var fieldEntries = new List<FieldEntry>();
			fieldEntries.Add(new FieldEntry { DisplayName = "MyFirstField", FieldIdentifier = "FieldOne", IsIdentifier = true });
			fieldEntries.Add(new FieldEntry { DisplayName = "MySecondField", FieldIdentifier = "FieldTwo" });
			return fieldEntries;
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			return GetDataReader();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			return GetDataReader();
		}

		private IDataReader GetDataReader()
		{
			var dt = new DataTable();
			dt.Columns.Add("FieldOne");
			dt.Columns.Add("FieldTwo");
			for (var i = 0; i < 10; i++)
			{
				var row = dt.NewRow();
				row["FieldOne"] = "Name" + i;
				row["FieldTwo"] = "SomeValue" + i;
				dt.Rows.Add(row);
			}
			return dt.CreateDataReader();
		}


	}
}
