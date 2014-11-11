using System.Collections.Generic;
using System.Data;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class JsonDataSource : IDataSource
	{
		private readonly string _source;
		public JsonDataSource(string source)
		{
			_source = source;
		}

		public IDataReader GetData(IEnumerable<FieldEntry> entries)
		{
			return new JsonDataReader(_source);
		}
	}
}
