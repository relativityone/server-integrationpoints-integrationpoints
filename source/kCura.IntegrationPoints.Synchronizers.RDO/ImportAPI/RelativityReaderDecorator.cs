using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	/// <summary>
	/// The objective of this class is to allow dynamic reading of sources to destination in import api.
	/// NOTE : The assumption is that the column names are artifact id of the fields due to the prior implementation of the framework.
	/// </summary>
	public class RelativityReaderDecorator : IDataReader
	{
		private readonly IDataReader _source;

		private readonly Dictionary<string, string> _targetNameToSourceIdentifier;
		private readonly Dictionary<string, string> _sourceIdentifierToTargetName; 
		private readonly HashSet<string> _identifiers; 

		public RelativityReaderDecorator(IDataReader sourceReader, FieldMap[] mappingFields)
		{
			_targetNameToSourceIdentifier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_sourceIdentifierToTargetName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			_identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_source = sourceReader;
			// current assumption is that source reader use fieldIdentifier as its column name
			for (int i = 0; i < mappingFields.Length; i++)
			{
				FieldMap map = mappingFields[i];
				if (map.FieldMapType == FieldMapTypeEnum.NativeFilePath)
				{
					const string nativeFileDestinationField = "NATIVE_FILE_PATH_001";
					_targetNameToSourceIdentifier[nativeFileDestinationField] = map.SourceField.FieldIdentifier;
					_sourceIdentifierToTargetName[map.SourceField.FieldIdentifier] = nativeFileDestinationField;
				}
				else if (map.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
				{
					_targetNameToSourceIdentifier[map.SourceField.ActualName] = map.SourceField.FieldIdentifier;
					_sourceIdentifierToTargetName[map.SourceField.FieldIdentifier] = map.SourceField.ActualName;
				}
				else if (map.DestinationField != null && map.SourceField != null)
				{
					_targetNameToSourceIdentifier[map.DestinationField.ActualName] = map.SourceField.FieldIdentifier;
					_sourceIdentifierToTargetName[map.SourceField.FieldIdentifier] = map.DestinationField.ActualName;

					// there should be only one, but the existing model of data structure are allowing multiple identifier fields.
					if (map.DestinationField.IsIdentifier)
					{
						_identifiers.Add(map.DestinationField.ActualName);
					}
				}
			}
		}

		public object this[string name]
		{
			get
			{
				if (_targetNameToSourceIdentifier.ContainsKey(name) == false)
				{
					throw new IndexOutOfRangeException(String.Format("{0} does not exist in the data table", name));
				}
				string sourceName = _targetNameToSourceIdentifier[name];
				object result = _source[sourceName];
				if ((result == null || result == DBNull.Value) && _identifiers.Contains(name))
				{
					throw new IndexOutOfRangeException(String.Format("Identifier[{0}] must have a value.", name));
				}
				return result;
			}
		}

		public object this[int i]
		{
			get
			{
				string sourceName = GetName(i);
				return this[sourceName];
			}
		}

		public int Depth
		{
			get { return _source.Depth; }
		}

		public int FieldCount
		{
			get { return _source.FieldCount; }
		}

		public bool IsClosed
		{
			get { return _source.IsClosed; }
		}

		public int RecordsAffected
		{
			get { return _source.RecordsAffected; }
		}

		public void Close()
		{
			_source.Close();
		}

		public void Dispose()
		{
			_source.Dispose();
		}

		public bool GetBoolean(int i)
		{
			return _source.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return _source.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _source.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return _source.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return _source.GetChars(i, bufferoffset, buffer, bufferoffset, length);
		}

		public IDataReader GetData(int i)
		{
			return _source.GetData(i);
		}

		public string GetDataTypeName(int i)
		{
			return _source.GetDataTypeName(i);
		}

		public DateTime GetDateTime(int i)
		{
			return _source.GetDateTime(i);
		}

		public decimal GetDecimal(int i)
		{
			return _source.GetDecimal(i);
		}

		public double GetDouble(int i)
		{
			return _source.GetDouble(i);
		}

		public Type GetFieldType(int i)
		{
			return _source.GetFieldType(i);
		}

		public float GetFloat(int i)
		{
			return _source.GetFloat(i);
		}

		public Guid GetGuid(int i)
		{
			return _source.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return _source.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return _source.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return _source.GetInt64(i);
		}

		public string GetName(int i)
		{
			if (FieldCount <= i)
			{
				throw new IndexOutOfRangeException(String.Format("Ordinal [{0}] does not exist in the data table", i));
			}
			string sourceName =  _source.GetName(i);
			return _sourceIdentifierToTargetName[sourceName];
		}

		public int GetOrdinal(string name)
		{
			if (_targetNameToSourceIdentifier.ContainsKey(name))
			{
				string sourceIdentifier = _targetNameToSourceIdentifier[name];
				return _source.GetOrdinal(sourceIdentifier);
			}
			throw new IndexOutOfRangeException(String.Format("{0} does not exist in the data table", name));
		}

		public DataTable GetSchemaTable()
		{
			return _source.GetSchemaTable();
		}

		public string GetString(int i)
		{
			return _source.GetString(i);
		}

		public object GetValue(int i)
		{
			return _source.GetValue(i);
		}

		public int GetValues(object[] values)
		{
			return _source.GetValues(values);
		}

		public bool IsDBNull(int i)
		{
			return _source.IsDBNull(i);
		}

		public bool NextResult()
		{
			return _source.NextResult();
		}

		public bool Read()
		{
			return _source.Read();
		}
	}
}