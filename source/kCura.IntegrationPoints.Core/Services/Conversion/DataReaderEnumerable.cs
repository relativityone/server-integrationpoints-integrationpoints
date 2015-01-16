using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Conversion;

namespace kCura.IntegrationPoints.Core.Conversion
{
	public class DataReaderToEnumerableService
	{
		private IObjectBuilder _objectBuilder;
		public DataReaderToEnumerableService(IObjectBuilder objectBuilder)
		{
			_objectBuilder = objectBuilder;
		}
		public IEnumerable<T> GetData<T>(IDataReader reader)
		{
			try
			{
				//this was not getting me the correct table columns it was giving me some bs column names that made no sense
				//DataColumnCollection columns = reader.GetSchemaTable().Columns;
				//found http://stackoverflow.com/questions/681653/can-you-get-the-column-names-from-a-sqldatareader
				var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
				while (reader.Read())
				{
					yield return _objectBuilder.BuildObject<T>(reader, columns);
				}
			}
			finally
			{
				reader.Dispose();
			}
		}
	}
}
