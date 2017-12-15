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
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetName(i);
		}

		public string GetDataTypeName(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetDataTypeName(i);
		}

		public Type GetFieldType(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetFieldType(i);
		}

		public object GetValue(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetValue(i);
		}

		public int GetValues(object[] values)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetValues(values);
		}

		public int GetOrdinal(string name)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetOrdinal(name);
		}

		public bool GetBoolean(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public Guid GetGuid(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetDouble(i);
		}

		public string GetString(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetString(i);
		}

		public decimal GetDecimal(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetDateTime(i);
		}

		public IDataReader GetData(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetData(i);
		}

		public bool IsDBNull(int i)
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.IsDBNull(i);
		}

		public int FieldCount
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation.FieldCount;
			}
		}

		object IDataRecord.this[int i]
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation[i];
			}
		}

		object IDataRecord.this[string name]
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation[name];
			}
		}

		public void Close()
		{
			using (new SerilogContextRestorer())
				_dataReaderImplementation.Close();
		}

		public DataTable GetSchemaTable()
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.GetSchemaTable();
		}

		public bool NextResult()
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.NextResult();
		}

		public bool Read()
		{
			using (new SerilogContextRestorer())
				return _dataReaderImplementation.Read();
		}

		public int Depth
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation.Depth;
			}
		}

		public bool IsClosed
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation.IsClosed;
			}
		}

		public int RecordsAffected
		{
			get
			{
				using (new SerilogContextRestorer())
					return _dataReaderImplementation.RecordsAffected;
			}
		}
	}
}
