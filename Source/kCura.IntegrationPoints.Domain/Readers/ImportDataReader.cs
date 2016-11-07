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
        private IAPILog _logger;
        private bool _isClosed;
        private IDataReader _sourceDataReader;
        private Dictionary<string, int> _nameToOrdinalMap; //map publicly available column names ==> source data reader ordinals
        private Dictionary<int, int> _ordinalMap; //map publicly available ordinals ==> source data reader ordinals
        private DataTable _schemaTable;

        public ImportDataReader(
            FieldMap[] fieldMaps,
            IDataSourceProvider sourceProvider,
            List<FieldEntry> sourceFields,
            List<string> entryIds,
            string sourceConfiguration,
            IAPILog logger)
        {
            _logger = logger;
            _isClosed = false;
            _sourceDataReader = sourceProvider.GetData(sourceFields, entryIds, sourceConfiguration);
            _nameToOrdinalMap = new Dictionary<string, int>();
            _ordinalMap = new Dictionary<int, int>();
            _schemaTable = new DataTable();
            Setup(fieldMaps);
        }

        private void Setup(FieldMap[] fieldMaps)
        {
            int curColIdx = 0;
            foreach (FieldMap cur in fieldMaps)
            {
                string curColName = string.Empty;

                //special cases
                if (cur.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
                {
                    //1. Add the special folderpath field Guid as a column
                    curColName = kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD;
                    _schemaTable.Columns.Add(curColName);

                    //2. Get the ordinal of the underlying source data reader column containing this info
                    int srcColIdx = _sourceDataReader.GetOrdinal(cur.SourceField.FieldIdentifier);

                    //3. Set up the maps
                    _ordinalMap[curColIdx] = srcColIdx;
                    _nameToOrdinalMap[curColName] = srcColIdx;

                    curColIdx++;

                    //4. Check whether this field is also mapped to a document field; if so, it needs a column as well
                    if (cur.DestinationField.FieldIdentifier != null)
                    {
                        //Get the underlying source name and add as a column
                        curColName = cur.SourceField.FieldIdentifier;
                        _schemaTable.Columns.Add(curColName);

                        //Set up the maps
                        _ordinalMap[curColIdx] = srcColIdx;
                        _nameToOrdinalMap[curColName] = srcColIdx;

                        curColIdx++;
                    }
                }
                //TODO: implement Native docs support
                // else if (cur.FieldMapType == FieldMapTypeEnum.NativeFilePath)

                //general case
                else
                {
                    curColName = cur.SourceField.FieldIdentifier;
                    _schemaTable.Columns.Add(curColName);

                    int srcColIdx = _sourceDataReader.GetOrdinal(curColName);

                    _ordinalMap[curColIdx] = srcColIdx;
                    _nameToOrdinalMap[curColName] = srcColIdx;

                    curColIdx++;
                }
            }
        }

		public object GetValue(int i)
		{
            return _sourceDataReader.GetValue(_ordinalMap[i]);
		}

		public int GetOrdinal(string name)
		{
            return _nameToOrdinalMap[name];
		}

		public string GetName(int i)
		{
            return _schemaTable.Columns[i].ColumnName;
		}

        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>

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
			throw new System.NotImplementedException();
		}

		public char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			// This is used to expose nested tables and other hierarchical data but currently this is not desired
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			return (GetValue(i) is System.DBNull);
		}

		public bool NextResult()
		{
            throw new NotImplementedException();
		}

		// Following this example: https://msdn.microsoft.com/en-us/library/aa720693(v=vs.71).aspx -- biedrzycki: Jan 20th, 2016
		public void Dispose()
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					this.Close();
				}
				catch (Exception e)
				{
					throw new SystemException("An exception of type " + e.GetType() +
											  " was encountered while closing the " + this.GetType().Name);
				}
			}
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
            throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
            throw new NotImplementedException();
		}
    }
}
