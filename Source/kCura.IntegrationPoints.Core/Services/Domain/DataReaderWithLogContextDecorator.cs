using System;
using System.Data;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class DataReaderWithLogContextDecorator : IDataReader
	{
		private readonly IDataReader _dataReaderImplementation;
		private readonly IDisposable _contextRestorer;

		public DataReaderWithLogContextDecorator(IDataReader decoratedDataReader)
		{
			_dataReaderImplementation = decoratedDataReader;
			_contextRestorer = new SerilogContextRestorer();
		}

		public void Dispose()
		{
			_dataReaderImplementation.Dispose();
			_contextRestorer.Dispose();
		}

		public string GetName(int i)
		{
			return _dataReaderImplementation.GetName(i);
		}

		public string GetDataTypeName(int i)
		{
			return _dataReaderImplementation.GetDataTypeName(i);
		}

		public Type GetFieldType(int i)
		{
			return _dataReaderImplementation.GetFieldType(i);
		}

		public object GetValue(int i)
		{
			return _dataReaderImplementation.GetValue(i);
		}

		public int GetValues(object[] values)
		{
			return _dataReaderImplementation.GetValues(values);
		}

		public int GetOrdinal(string name)
		{
			return _dataReaderImplementation.GetOrdinal(name);
		}

		public bool GetBoolean(int i)
		{
			return _dataReaderImplementation.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return _dataReaderImplementation.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _dataReaderImplementation.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return _dataReaderImplementation.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return _dataReaderImplementation.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public Guid GetGuid(int i)
		{
			return _dataReaderImplementation.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return _dataReaderImplementation.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return _dataReaderImplementation.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return _dataReaderImplementation.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			return _dataReaderImplementation.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			return _dataReaderImplementation.GetDouble(i);
		}

		public string GetString(int i)
		{
			return _dataReaderImplementation.GetString(i);
		}

		public decimal GetDecimal(int i)
		{
			return _dataReaderImplementation.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			return _dataReaderImplementation.GetDateTime(i);
		}

		public IDataReader GetData(int i)
		{
			return _dataReaderImplementation.GetData(i);
		}

		public bool IsDBNull(int i)
		{
			return _dataReaderImplementation.IsDBNull(i);
		}

		public int FieldCount
		{
			get { return _dataReaderImplementation.FieldCount; }
		}

		object IDataRecord.this[int i]
		{
			get { return _dataReaderImplementation[i]; }
		}

		object IDataRecord.this[string name]
		{
			get { return _dataReaderImplementation[name]; }
		}

		public void Close()
		{
			_dataReaderImplementation.Close();
		}

		public DataTable GetSchemaTable()
		{
			return _dataReaderImplementation.GetSchemaTable();
		}

		public bool NextResult()
		{
			return _dataReaderImplementation.NextResult();
		}

		public bool Read()
		{
			return _dataReaderImplementation.Read();
		}

		public int Depth
		{
			get { return _dataReaderImplementation.Depth; }
		}

		public bool IsClosed
		{
			get { return _dataReaderImplementation.IsClosed; }
		}

		public int RecordsAffected
		{
			get { return _dataReaderImplementation.RecordsAffected; }
		}
	}
}
