using Relativity.IntegrationPoints.Contracts.Models;
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
			List<T> dataSet = new List<T>();			
			List<string> columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
			while (reader.Read())
			{
				dataSet.Add(_objectBuilder.BuildObject<T>(reader, columns));
			}
			return dataSet;
		}
	}
}
