﻿using System;
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
        private DataTable _schemaTable;
        private int _folderPathFieldSourceColumnId;
		private Dictionary<string, int> KnownOrdinalDictionary;

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
            _schemaTable = new DataTable();
            _schemaTable.Columns.AddRange(GenerateDataColumnsFromFieldEntries(fieldMaps));
            KnownOrdinalDictionary = new Dictionary<string, int>();


            _logger.LogInformation("ImportDataReader constructor: My Schema Table Columns: {Joined}", string.Join("|", from c in _schemaTable.Columns.Cast<DataColumn>() select c.ColumnName));
            _logger.LogInformation("ImportDataReader constructor: Source ...Columns");
            _logger.LogInformation("Source Data Reader Field Count {FieldCount}", _sourceDataReader.FieldCount);

            for (int i = 0; i < _sourceDataReader.FieldCount; i++)
            {
                _logger.LogInformation("Source Column Index={i} name={ColumnName}", i, _sourceDataReader.GetName(i));
            }

            FieldMap folderPathInformationField = fieldMaps.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
                _folderPathFieldSourceColumnId = _sourceDataReader.GetOrdinal(folderPathInformationField.SourceField.FieldIdentifier);
                _logger.LogInformation("ImportDataReader constructor: assigned _folderPathFieldSourceColumnId == {id}", _folderPathFieldSourceColumnId);
			}
        }

        //Adapted from DocumentTransferDataReader.cs
		private static DataColumn[] GenerateDataColumnsFromFieldEntries(FieldMap[] mappingFields)
		{
			List<FieldEntry> fields = mappingFields.Select(field => field.SourceField).ToList();

			// disable native file location for this test

            /*
			fields.Add(new FieldEntry()
			{
				DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
				FieldType = FieldType.String
			});
			fields.Add(new FieldEntry
			{
				DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD,
				FieldType = FieldType.String
			});
            */

			// in case we found folder path info
			FieldMap folderPathInformationField = mappingFields.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
				if (folderPathInformationField.DestinationField.FieldIdentifier == null)
				{
					fields.Remove(folderPathInformationField.SourceField);
				}

				fields.Add(new FieldEntry()
				{
					DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME,
					FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD,
					FieldType = FieldType.String
				});
			}

			return fields.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
		}

        //Adapted from DocumentTransferDataReader.cs
		public object GetValue(int i)
		{
			string fieldIdentifier = GetName(i);
            _logger.LogInformation("GetValue({i}) called GetName(): fieldIdentifier={fieldIdentifier}", i, fieldIdentifier);

			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);

			if (success)
			{
                var rv = _sourceDataReader.GetValue(i);
                _logger.LogInformation("GetValue({i}) returning {rv}", i, rv);
                return rv;
			}
			else if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
                var rv = _sourceDataReader.GetValue(_folderPathFieldSourceColumnId);
                _logger.LogInformation("GetValue({i}) returning {rv}", i, rv);
                return rv;
			}
			else
			{
                _logger.LogInformation("GetValue({i}) is throwing an exception: Data requested for column that does not exist");
                throw new InvalidOperationException(string.Format("Data requested for column that does not exist: Index={0}", i));
			}
		}

        //IDataReader / IDataRecord Implementation adapted from RelativityReaderBase

        public void Close()
        {
            _sourceDataReader.Close();
            _isClosed = true;
        }

		public object this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		public object this[int i] { get { return GetValue(i); } }

        public int Depth
        {
            get { return 0; }
        }

		public int FieldCount
		{
			get { return _schemaTable.Columns.Count; }
		}

		public bool IsClosed { get { return _isClosed; } }

		public virtual int RecordsAffected
		{
			// this feature if wanted can be easily added just was not at this point because we are not supporting batching at this point
			get { return -1; }
		}

		public virtual bool GetBoolean(int i)
		{
			return Convert.ToBoolean(GetValue(i));
		}

		public virtual byte GetByte(int i)
		{
			return Convert.ToByte(GetValue(i));
		}

		public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public virtual char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public virtual IDataReader GetData(int i)
		{
			// This is used to expose nested tables and other hierarchical data but currently this is not desired
			throw new System.NotImplementedException();
		}

		public virtual DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(GetValue(i));
		}

		public virtual decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetValue(i));
		}

		public virtual double GetDouble(int i)
		{
			return Convert.ToDouble(GetValue(i));
		}

		public virtual float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		public virtual Guid GetGuid(int i)
		{
			return Guid.Parse(GetString(i));
		}

		public virtual short GetInt16(int i)
		{
			return Convert.ToInt16(GetValue(i));
		}

		public virtual int GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		public virtual long GetInt64(int i)
		{
			return Convert.ToInt64(GetValue(i));
		}

		public string GetName(int i)
		{
            var rv =_schemaTable.Columns[i].ColumnName;
            _logger.LogInformation("GetName({i}) returning {rv}", i, rv);
            return rv;
		}

		public int GetOrdinal(string name)
		{
			if (!KnownOrdinalDictionary.ContainsKey(name))
			{
				DataColumn column = _schemaTable.Columns[name];
				if (column == null)
				{
					throw new IndexOutOfRangeException(String.Format("'{0}' is not a valid column", name));
				}

				int ordinal = _schemaTable.Columns[name].Ordinal;
				KnownOrdinalDictionary[name] = ordinal;
			}
            _logger.LogInformation("GetOrdinal({name}) returning {rv}", name, KnownOrdinalDictionary[name]);
			return KnownOrdinalDictionary[name];
		}

		public virtual DataTable GetSchemaTable()
		{
			return _schemaTable;
		}

		public virtual string GetString(int i)
		{
			return Convert.ToString(GetValue(i));
		}

		public virtual int GetValues(object[] values)
		{
			throw new System.NotImplementedException();
		}

		public virtual bool IsDBNull(int i)
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
