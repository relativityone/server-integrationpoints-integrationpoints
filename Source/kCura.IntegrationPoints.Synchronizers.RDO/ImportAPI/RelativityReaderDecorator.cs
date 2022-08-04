using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
    /// <summary>
    /// The objective of this class is to allow dynamic reading of sources to destination in import api.
    /// NOTE : The assumption is that the column names are artifact id of the fields due to the prior implementation of the framework.
    /// </summary>
    public class RelativityReaderDecorator : IDataReader, IArtifactReader
    {
        private readonly Dictionary<string, string> _sourceIdentifierToTargetName; 
        private readonly Dictionary<string, string> _targetNameToSourceIdentifier;
        private readonly HashSet<string> _identifiers; 

        protected IDataReader _source;

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
                if (map.DestinationField.FieldIdentifier != null && map.SourceField.FieldIdentifier != null)
                {
                    RegisterField(map.DestinationField.ActualName, map.SourceField.FieldIdentifier);

                    // there should be only one, but the existing model of data structure are allowing multiple identifier fields.
                    if (map.DestinationField.IsIdentifier)
                    {
                        _identifiers.Add(map.DestinationField.ActualName);
                    }
                }
                else
                {
                    if (map.FieldMapType == FieldMapTypeEnum.NativeFilePath)
                    {
                        RegisterField(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME, map.SourceField.FieldIdentifier);
                    }
                    else if (map.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
                    {
                        RegisterField(map.SourceField.ActualName, map.SourceField.FieldIdentifier);
                    }
                }
            }

            // if the data reader contains the special fields native file path location field,
            // then we will use this as a way to map native file path location
            // this is only used when the reader is associate with native fields.
            HashSet<string> columns = new HashSet<string>(Enumerable.Range(0, sourceReader.FieldCount).Select(sourceReader.GetName));
            RegisterSpecialField(columns, Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME, Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_FILE_TYPE_FIELD_NAME, Constants.SPECIAL_FILE_TYPE_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD_NAME, Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME, Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_FOLDERPATH_FIELD_NAME, Constants.SPECIAL_FOLDERPATH_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME, Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD);
            
            RegisterSpecialField(columns, Constants.SPECIAL_IMAGE_FILE_NAME_FIELD_NAME, Constants.SPECIAL_IMAGE_FILE_NAME_FIELD);

            // So that the destination workspace file icons correctly display, we give the import API the file name of the document
            RegisterSpecialField(columns, Constants.SPECIAL_FILE_NAME_FIELD_NAME, Constants.SPECIAL_FILE_NAME_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, Constants.SPECIAL_SOURCEWORKSPACE_FIELD);
            RegisterSpecialField(columns, Constants.SPECIAL_SOURCEJOB_FIELD_NAME, Constants.SPECIAL_SOURCEJOB_FIELD);
        }

        private void RegisterSpecialField(HashSet<string> columns, string targetName, string sourceIdentifier)
        {
            if (columns.Contains(sourceIdentifier))
            {
                RegisterField(targetName, sourceIdentifier);
            }
        }

        private void RegisterField(string targetName, string sourceIdentifier)
        {
            _targetNameToSourceIdentifier[targetName] = sourceIdentifier;
            _sourceIdentifierToTargetName[sourceIdentifier] = targetName;
        }

        public object this[string name]
        {
            get
            {
                if (!_targetNameToSourceIdentifier.ContainsKey(name))
                {
                    throw new IndexOutOfRangeException($"{name} does not exist in the data table");
                }
                string sourceName = _targetNameToSourceIdentifier[name];
                object result = _source[sourceName];
                if ((result == null || result == DBNull.Value) && _identifiers.Contains(name))
                {
                    throw new IndexOutOfRangeException($"Identifier[{name}] must have a value.");
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

        public int Depth => _source.Depth;

        public int FieldCount => _source.FieldCount;

        public bool IsClosed => _source.IsClosed;

        public int RecordsAffected => _source.RecordsAffected;

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
                throw new IndexOutOfRangeException($"Ordinal [{i}] does not exist in the data table");
            }

            string sourceName =  _source.GetName(i);
            string targetName = _sourceIdentifierToTargetName[sourceName];
            return targetName;
        }

        public int GetOrdinal(string name)
        {
            if (_targetNameToSourceIdentifier.ContainsKey(name))
            {
                string sourceIdentifier = _targetNameToSourceIdentifier[name];
                return _source.GetOrdinal(sourceIdentifier);
            }
            throw new IndexOutOfRangeException($"{name} does not exist in the data table");
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

        //IArtifactReader
        public string ManageErrorRecords(string errorMessageFileLocation, string prePushErrorLineNumbersFileName)
        {
            IArtifactReader sourceArtifactReader = _source as IArtifactReader;
            return sourceArtifactReader?.ManageErrorRecords(errorMessageFileLocation, prePushErrorLineNumbersFileName);
        }

        public long? CountRecords()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public ArtifactFieldCollection ReadArtifact()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public string[] GetColumnNames(object args)
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public void ValidateColumnNames(Action<string> invalidNameAction)
        { }

        public string SourceIdentifierValue()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public void AdvanceRecord()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public void OnFatalErrorState()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public void Halt()
        {
            throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
        }

        public bool HasMoreRecords
        {
            get
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        public int CurrentLineNumber
        {
            get
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        public long SizeInBytes
        {
            get
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        public long BytesProcessed
        {
            get
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        event IArtifactReader.OnIoWarningEventHandler IArtifactReader.OnIoWarning
        {
            add
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }

            remove
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        event IArtifactReader.DataSourcePrepEventHandler IArtifactReader.DataSourcePrep
        {
            add
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }

            remove
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        event IArtifactReader.StatusMessageEventHandler IArtifactReader.StatusMessage
        {
            add
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }

            remove
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }

        event IArtifactReader.FieldMappedEventHandler IArtifactReader.FieldMapped
        {
            add
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }

            remove
            {
                throw new NotImplementedException("IArtifactReader calls should not be made to RelativityReaderDecorator");
            }
        }
    }
}
