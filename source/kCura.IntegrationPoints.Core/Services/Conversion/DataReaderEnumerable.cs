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

	//public class DataReaderEnumerable : IEnumerable<IDictionary<FieldEntry, object>>
	//{
	//	private IDataReader _dataReader;
	//	private IEnumerable<FieldMap> _fieldMap;

	//	public DataReaderEnumerable(IDataReader dataReader, IEnumerable<FieldMap> fieldMap)
	//	{
	//		_dataReader = dataReader;
	//		_fieldMap = fieldMap;
	//	}

	//	public IEnumerator<IDictionary<FieldEntry, object>> GetEnumerator()
	//	{
	//		return _dataReader.
	//	}

	//	IEnumerator IEnumerable.GetEnumerator()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}

	//public class DataReaderEnumerator : IEnumerator<IDictionary<FieldEntry, object>>
	//{
	//	private IDataReader _dataReader;
	//	private IEnumerable<FieldMap> _fieldMap;
	//	public DataReaderEnumerator(IDataReader dataReader, IEnumerable<FieldMap> fieldMap)
	//	{
	//		_dataReader = dataReader;
	//		_fieldMap = fieldMap;
	//	}

	//	public IDictionary<FieldEntry, object> Current
	//	{
	//		get { _dataReader.; }
	//	}

	//	public void Dispose()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	object IEnumerator.Current
	//	{
	//		get { throw new NotImplementedException(); }
	//	}

	//	public bool MoveNext()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public void Reset()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}
}
