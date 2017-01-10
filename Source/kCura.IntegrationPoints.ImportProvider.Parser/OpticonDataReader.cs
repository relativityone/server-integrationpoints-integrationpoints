using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using System;
using System.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class OpticonDataReader : IDataReader
	{
		private const string _RECORD_DELIMITER_STRING = ",";
		private ImageLoadFile _config;
		private OpticonFileReader _opticonFileReader;
		private bool _isClosed;
		private string _currentLine;
		private ulong _documentId;

		public OpticonDataReader(ImageLoadFile config)
		{
			_config = config;
			_isClosed = false;
			_currentLine = string.Empty;
			_documentId = 0;
		}

		public void Init()
		{
			_opticonFileReader = new OpticonFileReader(0, _config, null, Guid.Empty, false);
			_opticonFileReader.Initialize();
			_isClosed = !_opticonFileReader.HasMoreRecords;
		}

		private void ReadCurrentRecord()
		{
			ImageRecord currentRecord = _opticonFileReader.GetImageRecord();
			if (currentRecord.IsNewDoc)
			{
				_documentId++;
			}

			string[] result = new string[3];
			result[OpticonInfo.BATES_NUMBER_FIELD_INDEX] = currentRecord.BatesNumber;
			result[OpticonInfo.FILE_LOCATION_FIELD_INDEX] = currentRecord.FileLocation;
			result[OpticonInfo.DOCUMENT_ID_FIELD_INDEX] = _documentId.ToString();

			_currentLine = string.Join(_RECORD_DELIMITER_STRING, result);
		}

		//IDataReader Implementation

		public void Close()
		{
			_opticonFileReader.Close();
			_isClosed = true;
		}

		public int Depth
		{
			get { return 0; }
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool IsClosed
		{
			get { return _isClosed; }
		}

		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		public bool Read()
		{
			if (_opticonFileReader.HasMoreRecords)
			{
				ReadCurrentRecord();
				return true;
			}
			else
			{
				return false;
			}
		}

		public int FieldCount
		{
			get { return 0; }
		}

		public int RecordsAffected
		{
			get { throw new NotImplementedException(); }
		}

		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		public byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			throw new NotImplementedException();
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		public string GetName(int i)
		{
			throw new NotImplementedException();
		}

		public int GetOrdinal(string name)
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			return _currentLine;
		}

		public object GetValue(int i)
		{
			throw new NotImplementedException();
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		public object this[string name]
		{
			get { throw new NotImplementedException(); }
		}

		public object this[int i]
		{
			get { throw new NotImplementedException(); }
		}

		public void Dispose()
		{
			this.Close();
		}
	}
}
