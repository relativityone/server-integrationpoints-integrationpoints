﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class LoadFileDataReader : DataReaderBase, IArtifactReader
	{
		private bool _isClosed;
		private bool _extractedTextHasPathInfo;
		private bool _nativeFileHasPathInfo;
		private string _loadFileDirectory;
		private string[] _currentLine;
		private int _columnCount;
		private LoadFile _config;
		private LoadFileReader _loadFileReader;
		private DataTable _schemaTable;
		private Dictionary<string, int> _ordinalMap;
		ImportProviderSettings _providerSettings;

		public LoadFileDataReader(ImportProviderSettings providerSettings, LoadFile config, LoadFileReader reader)
		{
			_providerSettings = providerSettings;
			_config = config;
			_loadFileReader = reader;

			_isClosed = false;
			_columnCount = 0;
			_extractedTextHasPathInfo = !string.IsNullOrEmpty(_providerSettings.ExtractedTextPathFieldIdentifier);
			_nativeFileHasPathInfo = !string.IsNullOrEmpty(_providerSettings.NativeFilePathFieldIdentifier);
			_loadFileDirectory = Path.GetDirectoryName(_providerSettings.LoadFile);

			_schemaTable = new DataTable();
			_ordinalMap = new Dictionary<string, int>();
		}

		public void Init()
		{
			//Accessing the ColumnNames is necessary to properly intialize the loadFileReader;
			//Otherwise ReadArtifact() throws "Object reference not set to an instance of an object."
			_columnCount = _loadFileReader.GetColumnNames(_config).Length;
			for (int i = 0; i < _columnCount; i++)
			{
				string colName = i.ToString();
				_schemaTable.Columns.Add(colName);
				_ordinalMap[colName] = i;
			}
		}

		//Get a line stored for the current row, based on delimter settings in the _config
		private void ReadCurrentRecord()
		{
			_currentLine = new string[_columnCount];
			try
			{
				ArtifactFieldCollection artifacts = _loadFileReader.ReadArtifact();
				_currentLine = new string[artifacts.Count];
				foreach (ArtifactField artifact in artifacts)
				{
					string artifactValue = artifact.ValueAsString;
					if (((_extractedTextHasPathInfo && artifact.ArtifactID.ToString() == _providerSettings.ExtractedTextPathFieldIdentifier)
					|| (_nativeFileHasPathInfo && artifact.ArtifactID.ToString() == _providerSettings.NativeFilePathFieldIdentifier))
					&& !string.IsNullOrEmpty(artifactValue) //If the path is empty, there is no native and we shouldn't attempt to join the paths
					&& !Path.IsPathRooted(artifactValue)) //Do not rewrite paths if column contains full path info
					{
						_currentLine[artifact.ArtifactID] = Path.Combine(_loadFileDirectory, artifactValue);
					}
					else
					{
						_currentLine[artifact.ArtifactID] = artifactValue;
					}
				}
			}
			//This exception causes problems generating error file creation and can crash the ImportAPI job.
			//Catching this exception and blanking _currentLine causes a "Identity value not set" error in Job History Errors tab,
			//but the error file generation and document import proceeds correctly. 
			catch (kCura.WinEDDS.LoadFileBase.ColumnCountMismatchException)
			{
				for (int i = 0; i < _currentLine.Length; i++)
				{
					_currentLine[i] = string.Empty;
				}
			}
		}

		public override int FieldCount
		{
			get
			{
				return _schemaTable.Columns.Count;
			}
		}

		public override bool IsClosed
		{
			get
			{
				return _isClosed;
			}
		}

		public override void Close()
		{
			_isClosed = true;
			_loadFileReader.Close();
		}

		public override string GetName(int i)
		{
			return _schemaTable.Columns[i].ColumnName;
		}

		public override int GetOrdinal(string name)
		{
			return _ordinalMap[name];
		}

		public override DataTable GetSchemaTable()
		{
			return _schemaTable;
		}

		public override object GetValue(int i)
		{
			return _currentLine[i];
		}

		public override bool Read()
		{
			if (_loadFileReader.HasMoreRecords)
			{
				ReadCurrentRecord();
				return true;
			}
			else
			{
				return false;
			}
		}

		public override string GetDataTypeName(int i)
		{
			throw new NotImplementedException("IDataReader.GetDataTypeName should not be called on LoadFileDataReader");
		}

		public override Type GetFieldType(int i)
		{
			throw new NotImplementedException("IDataReader.GetFieldType should not be called on LoadFileDataReader");
		}

		//IArtifactReader implementation

		public string ManageErrorRecords(string errorMessageFileLocation, string prePushErrorLineNumbersFileName)
		{
			return ((IArtifactReader)_loadFileReader).ManageErrorRecords(errorMessageFileLocation, prePushErrorLineNumbersFileName);
		}

		public long CountRecords()
		{
			return _loadFileReader.CountRecords();
		}

		public ArtifactFieldCollection ReadArtifact()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public string[] GetColumnNames(object args)
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public string SourceIdentifierValue()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public void AdvanceRecord()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public void OnFatalErrorState()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public void Halt()
		{
			throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
		}

		public bool HasMoreRecords
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
			}
		}

		public int CurrentLineNumber
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
			}
		}

		public long SizeInBytes
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
			}
		}

		public long BytesProcessed
		{
			get
			{
				throw new NotImplementedException("IArtifactReader calls should not be made to LoadFileDataReader");
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
