using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.LDAPSync.prototype.datasources.Implementations;
using Newtonsoft.Json;

namespace kCura.LDAPSync.prototype.datasources
{
	public class Worker
	{
		private readonly IDataConverterFactory _converterFactory;
		private readonly IDataSourceFactory _sourceFactory;
		public Worker(IDataConverterFactory converterFactory, IDataSourceFactory sourceFactory)
		{
			_converterFactory = converterFactory;
			_sourceFactory = sourceFactory;
		}

		private bool HasBatchToProcess()
		{
			return Data.BatchTable.Any();
		}

		public IEnumerable<string> GetBatchIds()
		{
			return Data.BatchTable.Pop();
		}

		public IEnumerable<FieldMap> GetFieldMap()
		{
			IEnumerable<FieldMap> map;
			using (var r = new StreamReader("fieldMapping.json"))
			{
				string json = r.ReadToEnd();
				map = JsonConvert.DeserializeObject<List<FieldMap>>(json);
			}
			return map;
		}

		private string GetDestinationConfig()
		{
			return string.Empty;
		}

		public void Execute()
		{
			while (HasBatchToProcess())
			{
				List<FieldMap> fieldMap = GetFieldMap().ToList();

				IDataSourceProvider dataSource = _sourceFactory.GetDataSource();
				IEnumerable<FieldEntry> fieldsToRead = fieldMap.Select(x => x.SourceField).ToList();
				IEnumerable<string> batchIds = GetBatchIds();
				var config = GetDestinationConfig();
				IDataReader sourceReader = dataSource.GetData(fieldsToRead, batchIds, config);
				
				var dt = new DataTable();
				dt.Load(sourceReader);

				var converter = _converterFactory.GetConverter();
				converter.SyncData(new BsEnumerable(dt, fieldsToRead), fieldMap);

			}

		}
	}
}
