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
			string fields = _helper.ReadFields(options);
			return JsonConvert.DeserializeObject<List<FieldEntry>>(fields);
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			string file = _helper.ReadData(options);
			List<DataObject> objects = JsonConvert.DeserializeObject<List<DataObject>>(file);
			using (DataTable dataTable = objects.ToBatchableIds(identifier.FieldIdentifier))
			{
				return dataTable.CreateDataReader();
			}
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			string file = _helper.ReadData(options);

			FieldEntry[] fieldEntries = fields as FieldEntry[] ?? fields.ToArray();
			IEnumerable<string> fieldList = fieldEntries.Select(f => f.FieldIdentifier);
			string identifier = fieldEntries.First(f => f.IsIdentifier).FieldIdentifier;

			List<DataObject> objects = JsonConvert.DeserializeObject<List<DataObject>>(file);
			HashSet<string> ids = new HashSet<string>(entryIds);

			using (DataTable dataTable = objects.ToDataTable(identifier, fieldList, ids))
			{
				return dataTable.CreateDataReader();
			}
		}
	}
}