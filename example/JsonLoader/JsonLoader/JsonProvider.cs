using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using Newtonsoft.Json;

namespace JsonLoader
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider("4380b80b-57ef-48c3-bf02-b98d2855166b")]
	public class JsonProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
	{
		private readonly JsonHelper _helper;
		
		public JsonProvider(JsonHelper helper)
		{
			_helper = helper;
		}
		
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var fields = _helper.ReadFile(options);
			return JsonConvert.DeserializeObject<List<FieldEntry>>(fields);
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			throw new NotImplementedException();
		}
	}
}
