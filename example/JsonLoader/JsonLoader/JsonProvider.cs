﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using Newtonsoft.Json;

namespace JsonLoader
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider(GlobalConst.JSON_SOURCE_PROVIDER_GUID)]
	public class JsonProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
	{

		private readonly JsonHelper _helper;

		public JsonProvider(JsonHelper helper)
		{
			_helper = helper;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var fields = _helper.ReadFields(options);
			return JsonConvert.DeserializeObject<List<FieldEntry>>(fields);
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			var file = _helper.ReadData(options);
			var obj = JsonConvert.DeserializeObject<List<DataObject>>(file);
			var dt = obj.ToDataTable();
			var columns = dt.CreateDataReader().GetSchemaTable().Columns;
			var reader = dt.CreateDataReader();
			var mydt = new DataTable();
			var cs = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
			mydt.Load(dt.CreateDataReader());
			foreach (var column in columns)
			{

			}
			return dt.CreateDataReader();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			var file = _helper.ReadData(options);
			var obj = JsonConvert.DeserializeObject<List<DataObject>>(file);
			var dt = obj.ToDataTable();
			var columns = dt.CreateDataReader().GetSchemaTable().Columns;
			return dt.CreateDataReader();
		}
	}
}
