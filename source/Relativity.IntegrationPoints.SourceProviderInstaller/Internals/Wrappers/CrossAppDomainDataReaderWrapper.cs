using System;
using System.Data;
using System.Runtime.Remoting;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers
{
	/// <summary>
	/// This class wraps <see cref="IDataReader"/> from another AppDomain
	/// <see cref="CrossAppDomainDataReaderWrapper"/> objects shouldn't be used directly in parent's AppDomain because of issues
	/// with multiple calls to <see cref="Dispose()"/>. Proxy in parent AppDomain tries to call Dispose in child AppDomain
	/// but this objects was already disposed there, so <see cref="ObjectDisposedException"/> is thrown. Instead we should
	/// use <see cref="SafeDisposingDataReaderWrapper"/> which guarantees proper IDisposable implementation.
	/// </summary>
	/// <inheritdoc cref="MarshalByRefObject"/>
	/// <inheritdoc cref="IDataReader"/>
	internal class CrossAppDomainDataReaderWrapper : MarshalByRefObject, IDataReader
	{
		private bool _isDisposed;
		private readonly IDataReader _dataReader;

		internal CrossAppDomainDataReaderWrapper(IDataReader dataReader)
		{
			_dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
		}

		#region Decorated Methods
		public void Close()
		{
			_dataReader.Close();
		}

		public int Depth
		{
			get { return _dataReader.Depth; }
		}

		public DataTable GetSchemaTable()
		{
			return _dataReader.GetSchemaTable();
		}

		public bool IsClosed
		{
			get { return _dataReader.IsClosed; }
		}

		public bool NextResult()
		{
			return _dataReader.NextResult();
		}

		public bool Read()
		{
			return _dataReader.Read();
		}

		public int RecordsAffected
		{
			get { return _dataReader.RecordsAffected; }
		}

		public int FieldCount
		{
			get { return _dataReader.FieldCount; }
		}

		public bool GetBoolean(int i)
		{
			return _dataReader.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return _dataReader.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return _dataReader.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return _dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public IDataReader GetData(int i)
		{
			return _dataReader.GetData(i);
		}

		public string GetDataTypeName(int i)
		{
			return _dataReader.GetDataTypeName(i);
		}

		public DateTime GetDateTime(int i)
		{
			return _dataReader.GetDateTime(i);
		}

		public decimal GetDecimal(int i)
		{
			return _dataReader.GetDecimal(i);
		}

		public double GetDouble(int i)
		{
			return _dataReader.GetDouble(i);
		}

		public Type GetFieldType(int i)
		{
			return _dataReader.GetFieldType(i);
		}

		public float GetFloat(int i)
		{
			return _dataReader.GetFloat(i);
		}

		public Guid GetGuid(int i)
		{
			return _dataReader.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return _dataReader.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return _dataReader.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return _dataReader.GetInt64(i);
		}

		public string GetName(int i)
		{
			return _dataReader.GetName(i);
		}

		public int GetOrdinal(string name)
		{
			return _dataReader.GetOrdinal(name);
		}

		public string GetString(int i)
		{
			return _dataReader.GetString(i);
		}

		public object GetValue(int i)
		{
			return _dataReader.GetValue(i);
		}

		public int GetValues(object[] values)
		{
			return _dataReader.GetValues(values);
		}

		public bool IsDBNull(int i)
		{
			return _dataReader.IsDBNull(i);
		}

		public object this[string name]
		{
			get { return _dataReader[name]; }
		}

		public object this[int i]
		{
			get { return _dataReader[i]; }
		}
		#endregion

		#region Cross AppDomain communication
		public override object InitializeLifetimeService()
		{
			return null;
		}

		private void DisconnectFromRemoteObject()
		{
			RemotingServices.Disconnect(this);
		}
		#endregion

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			if (disposing)
			{
				_dataReader.Dispose();
			}

			DisconnectFromRemoteObject();
			_isDisposed = true;
		}

		~CrossAppDomainDataReaderWrapper()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
