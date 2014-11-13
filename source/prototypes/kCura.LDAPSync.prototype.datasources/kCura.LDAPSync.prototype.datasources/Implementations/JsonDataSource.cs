using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.LDAPSync.prototype.datasources.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class JsonDataSource : IDataSourceProvider
	{
		private readonly string _source;
		public JsonDataSource(string source)
		{
			_source = source;
		}

		public IDataReader GetData(IEnumerable<FieldEntry> entries)
		{
			return new JsonDataReader(_source, string.Empty);
		}

		public IEnumerable<FieldEntry> GetFields()
		{
			using (var r = new StreamReader(_source))
			{
				string json = r.ReadToEnd();
				var obj = JsonConvert.DeserializeObject<JObject>(json);
				var m = obj["fields"].Children();
				return m.Select(x => new FieldEntry
				{
					DisplayName = x.Value<string>(),
					FieldIdentifier = x.Value<string>(),
					FieldType = FieldType.String
				});
			}

		}

		public IDataReader GetData(IEnumerable<FieldEntry> entries, IEnumerable<string> entryIds, string config)
		{
			var jsonReader = new JsonDataReader(_source, string.Empty);
			var lookup = entryIds.ToDictionary(x => x, x => x);
			var rows = jsonReader.GetDataFromReader().Rows.Cast<DataRow>().Where(x => lookup.ContainsKey((string)x["name"]));
			var dt = new DataTable();
			dt.Columns.AddRange(entries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray());
			foreach (var row in rows)
			{
				dt.ImportRow(row);
			}
			return dt.CreateDataReader();
		}

		public IDataReader GetBatchableData(FieldEntry identifier, string options)
		{
			var jsonReader = new JsonDataReader(_source, string.Empty);
			var rows = jsonReader.GetDataFromReader().Rows.Cast<DataRow>().Select(x => x[identifier.FieldIdentifier] as string);
			var dt = new DataTable();
			dt.Columns.Add(new DataColumn(identifier.FieldIdentifier));
			foreach (var row in rows)
			{
				var r = dt.NewRow();
				r[0] = row;
				dt.Rows.Add(r);
			}
			return dt.CreateDataReader();
		}
	}
}
