using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.JsonLoader.Models;

namespace Relativity.IntegrationPoints.JsonLoader
{
	[DataSourceProvider(Constants.JSON_SOURCE_PROVIDER_GUID)]
	public class JsonProvider : IDataSourceProvider
	{
		private readonly JsonHelper _helper;

		public JsonProvider(JsonHelper helper)
		{
			_helper = helper;
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			string fields = _helper.ReadFields(providerConfiguration.Configuration);
			return JsonConvert.DeserializeObject<List<FieldEntry>>(fields);
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			string file = _helper.ReadData(providerConfiguration.Configuration);
			List<DataObject> objects = JsonConvert.DeserializeObject<List<DataObject>>(file);
			using (DataTable dataTable = objects.ToBatchableIds(identifier.FieldIdentifier))
			{
				return dataTable.CreateDataReader();
			}
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			string file = _helper.ReadData(providerConfiguration.Configuration);

			FieldEntry[] fieldEntries = fields as FieldEntry[] ?? fields.ToArray();
			IEnumerable<string> fieldList = fieldEntries.Select(f => f.FieldIdentifier);
			string identifier = fieldEntries.First(f => f.IsIdentifier).FieldIdentifier;

			List<DataObject> objects = JsonConvert.DeserializeObject<List<DataObject>>(file);
			HashSet<string> ids = new HashSet<string>(entryIds, StringComparer.OrdinalIgnoreCase);

			using (DataTable dataTable = objects.ToDataTable(identifier, fieldList, ids))
			{
				return dataTable.CreateDataReader();
			}
		}
	}
}