using System;
using System.Collections.Generic;
using System.Data;

using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportDataReader : DataReaderBase, IArtifactReader
	{
		private bool _isClosed;
		private IDataReader _sourceDataReader;
		private Dictionary<int, int> _ordinalMap; //map publicly available ordinals ==> source data reader ordinals
		private DataTable _schemaTable;

		public ImportDataReader(IDataReader sourceDataReader)
		{
			_isClosed = false;
			_sourceDataReader = sourceDataReader;
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
				} else if(cur.FieldMapType == FieldMapTypeEnum.NativeFilePath)
				{
					//Add special native file path column
					AddColumn(
						kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
						sourceOrdinal,
						curColIdx++);

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
		}

		public override object GetValue(int i)
		{
			return _sourceDataReader.GetValue(_ordinalMap[i]);
		}

		public override int GetOrdinal(string name)
		{
			return _schemaTable.Columns[name].Ordinal;
		}

		public override string GetName(int i)
		{
			return _schemaTable.Columns[i].ColumnName;
		}

		public override DataTable GetSchemaTable()
		{
			return _schemaTable;
		}

		public override int FieldCount
		{
			get { return _schemaTable.Columns.Count; }
		}

		public override bool IsClosed { get { return _isClosed; } }

		public override void Close()
		{
			_sourceDataReader.Close();
			_isClosed = true;
		}

		public override bool Read()
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
		public override string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		//IArtifactReader
		public string ManageErrorRecords(string errorMessageFileLocation, string prePushErrorLineNumbersFileName)
		{
			return ((IArtifactReader)_sourceDataReader).ManageErrorRecords(errorMessageFileLocation, prePushErrorLineNumbersFileName);
		}

		public long CountRecords()
		{
			return ((IArtifactReader)_sourceDataReader).CountRecords();
		}

		public ArtifactFieldCollection ReadArtifact()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public string[] GetColumnNames(object args)
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public string SourceIdentifierValue()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public void AdvanceRecord()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public void OnFatalErrorState()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public void Halt()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
		}

		public bool HasMoreRecords
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
			}
		}

		public int CurrentLineNumber
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
			}
		}

		public long SizeInBytes
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
			}
		}

		public long BytesProcessed
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to ImportDataReader");
			}
		}

		event IArtifactReader.OnIoWarningEventHandler IArtifactReader.OnIoWarning
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		event IArtifactReader.DataSourcePrepEventHandler IArtifactReader.DataSourcePrep
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		event IArtifactReader.StatusMessageEventHandler IArtifactReader.StatusMessage
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		event IArtifactReader.FieldMappedEventHandler IArtifactReader.FieldMapped
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}
	}
}
