using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Models;

using Relativity.API;

namespace kCura.IntegrationPoints.Domain.Readers
{
    public class ImportDataReader : IDataReader
    {
        private bool _isClosed;
        private IDataReader _sourceDataReader;
        private Dictionary<string, int> _nameToOrdinalMap; //map publicly available column names ==> source data reader ordinals
        private Dictionary<int, int> _ordinalMap; //map publicly available ordinals ==> source data reader ordinals
        private DataTable _schemaTable;

        public ImportDataReader(
            IDataSourceProvider sourceProvider,
            List<FieldEntry> sourceFields,
            List<string> entryIds,
            string sourceConfiguration)
        {
            _isClosed = false;
            _sourceDataReader = sourceProvider.GetData(sourceFields, entryIds, sourceConfiguration);
            _nameToOrdinalMap = new Dictionary<string, int>();
            _ordinalMap = new Dictionary<int, int>();
            _schemaTable = new DataTable();
        }

        public void Setup(FieldMap[] fieldMaps)
        {
            int curColIdx = 0;
            foreach (FieldMap cur in fieldMaps)
            {
                int sourceOrdinal = _sourceDataReader.GetOrdinal(cur.SourceField.FieldIdentifier);

                //special cases
                if (cur.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
                {
                    //Add special folder path column
                    AddColumn(
                        kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD,
                        sourceOrdinal,
                        curColIdx++);

                    //If field is also mapped to a document field, it needs a column as well
                    if (cur.DestinationField.FieldIdentifier != null)
                    {
                        AddColumn(
                            cur.SourceField.FieldIdentifier,
                            sourceOrdinal,
                            curColIdx++);
                    }
                }
                //general case
                else
                {
                    AddColumn(
                        cur.SourceField.FieldIdentifier,
                        sourceOrdinal,
                        curColIdx++);
                }
            }
        }

        private void AddColumn(string columnName, int srcColIdx, int curColIdx)
        {
            _schemaTable.Columns.Add(columnName);
            _ordinalMap[curColIdx] = srcColIdx;
            _nameToOrdinalMap[columnName] = srcColIdx;
        }

		public object GetValue(int i)
		{
            return _sourceDataReader.GetValue(_ordinalMap[i]);
		}

		public int GetOrdinal(string name)
		{
            return _schemaTable.Columns[name].Ordinal;
		}

		public string GetName(int i)
		{
            return _schemaTable.Columns[i].ColumnName;
		}

		public string GetString(int i)
		{
			return Convert.ToString(GetValue(i));
		}

		public DataTable GetSchemaTable()
		{
            return _schemaTable;
		}

		public int FieldCount
		{
			get { return _schemaTable.Columns.Count; }
		}

        public int Depth
        {
            get { return 0; }
        }

		public bool IsClosed { get { return _isClosed; } }

		public int RecordsAffected
		{
			// this feature if wanted can be easily added just was not at this point because we are not supporting batching at this point
			get { return -1; }
		}

		public bool GetBoolean(int i)
		{
			return Convert.ToBoolean(GetValue(i));
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(GetValue(i));
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.InvalidOperationException("IDataReader.GetBytes is not supported.");
		}

		public char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.InvalidOperationException("IDataReader.GetChars is not supported.");
		}

		public IDataReader GetData(int i)
		{
			// This is used to expose nested tables and other hierarchical data but currently this is not desired
			throw new System.InvalidOperationException("IDataReader.GetData is not supported.");
		}

		public DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(GetValue(i));
		}

		public decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetValue(i));
		}

		public double GetDouble(int i)
		{
			return Convert.ToDouble(GetValue(i));
		}

		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		public Guid GetGuid(int i)
		{
			return Guid.Parse(GetString(i));
		}

		public short GetInt16(int i)
		{
			return Convert.ToInt16(GetValue(i));
		}

		public int GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		public long GetInt64(int i)
		{
			return Convert.ToInt64(GetValue(i));
		}

		public int GetValues(object[] values)
		{
			throw new System.InvalidOperationException("IDataReader.GetValues is not supported.");
		}

		public bool IsDBNull(int i)
		{
			return (GetValue(i) is System.DBNull);
		}

		public bool NextResult()
		{
			throw new System.InvalidOperationException("IDataReader.NextResult is not supported.");
		}

		// Following this example: https://msdn.microsoft.com/en-us/library/aa720693(v=vs.71).aspx -- biedrzycki: Jan 20th, 2016
		public void Dispose()
		{
            this.Close();
			System.GC.SuppressFinalize(this);
		}

        public void Close()
        {
            _sourceDataReader.Close();
            _isClosed = true;
        }

		protected void FetchDataToRead()
		{
            _sourceDataReader.Read();
		}

		public bool Read()
		{
            if (_isClosed)
            {
                return false;
            }
            else
            {
                return _sourceDataReader.Read();
            }
		}

		public object this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		public object this[int i] { get { return GetValue(i); } }

		public string GetDataTypeName(int i)
		{
			throw new System.InvalidOperationException("IDataReader.GetDataTypeName is not supported.");
		}

		public Type GetFieldType(int i)
		{
			throw new System.InvalidOperationException("IDataReader.GetFieldType is not supported.");
		}
    }
}
