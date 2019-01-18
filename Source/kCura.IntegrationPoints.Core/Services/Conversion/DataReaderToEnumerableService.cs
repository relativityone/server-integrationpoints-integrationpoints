using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public class DataReaderToEnumerableService
	{
		private readonly IObjectBuilder _objectBuilder;
		public DataReaderToEnumerableService(IObjectBuilder objectBuilder)
		{
			_objectBuilder = objectBuilder;
		}

		public IEnumerable<T> GetData<T>(IDataReader reader)
		{
			List<string> columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
			while (reader.Read())
			{
				yield return _objectBuilder.BuildObject<T>(reader, columns);
			}
		}
	}
}
