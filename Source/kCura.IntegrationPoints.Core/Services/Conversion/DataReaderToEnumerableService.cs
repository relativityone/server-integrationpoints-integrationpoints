using System.Collections.Generic;
using System.Data;

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
			while (reader.Read())
			{
				yield return _objectBuilder.BuildObject<T>(reader);
			}
		}
	}
}
