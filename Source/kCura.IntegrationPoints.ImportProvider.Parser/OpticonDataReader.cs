using System.Collections.Generic;
using kCura.WinEDDS.Api;
using System;
using System.Data;
using System.IO;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class OpticonDataReader : DataReaderBase, IOpticonDataReader
	{
		private bool _isClosed;
		private ulong _documentId;
		private readonly DataTable _schemaTable;
		private readonly Dictionary<string, int> _ordinalMap;
		private readonly IImageReader _opticonFileReader;
		private readonly IJobStopManager _jobStopManager;
		private readonly string _loadFileDirectory;
		private readonly string[] _currentLine;

		public OpticonDataReader(ImportProviderSettings providerSettings, IImageReader reader, IJobStopManager jobStopManager)
		{
			_opticonFileReader = reader;
			_jobStopManager = jobStopManager;
			
			_isClosed = false;
			_documentId = 0;

			_loadFileDirectory = Path.GetDirectoryName(providerSettings.LoadFile);

			_schemaTable = new DataTable();
			_ordinalMap = new Dictionary<string, int>();
			_currentLine = new string[3];
		}

		public void Init()
		{
			_schemaTable.Columns.Add(OpticonInfo.BATES_NUMBER_FIELD_NAME);
			_schemaTable.Columns.Add(OpticonInfo.FILE_LOCATION_FIELD_NAME);
			_schemaTable.Columns.Add(OpticonInfo.DOCUMENT_ID_FIELD_NAME);
			_ordinalMap[OpticonInfo.BATES_NUMBER_FIELD_NAME] = 0;
			_ordinalMap[OpticonInfo.FILE_LOCATION_FIELD_NAME] = 1;
			_ordinalMap[OpticonInfo.DOCUMENT_ID_FIELD_NAME] = 2;

			_opticonFileReader.Initialize();
		}

		private ImageRecord ReadCurrentRecord()
		{
			ImageRecord currentRecord = _opticonFileReader.GetImageRecord();
			if (currentRecord.IsNewDoc)
			{
				_documentId++;
			}

			_currentLine[OpticonInfo.BATES_NUMBER_FIELD_INDEX] = currentRecord.BatesNumber;
			_currentLine[OpticonInfo.DOCUMENT_ID_FIELD_INDEX] = _documentId.ToString();
			string fileLocation = currentRecord.FileLocation;
			if (!Path.IsPathRooted(fileLocation))
			{
				_currentLine[OpticonInfo.FILE_LOCATION_FIELD_INDEX] = Path.Combine(_loadFileDirectory, fileLocation);
			}
			else
			{
				_currentLine[OpticonInfo.FILE_LOCATION_FIELD_INDEX] = fileLocation;
			}

			return currentRecord;
		}

		public long CountRecords()
		{
			return _opticonFileReader.CountRecords() ?? 0;
		}

		public override int FieldCount
		{
			get
			{
				return 3;
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
			_opticonFileReader.Close();
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
			if(!_opticonFileReader.HasMoreRecords)
			{
				return false;
			}

			ImageRecord record = ReadCurrentRecord();
			if(_jobStopManager?.ShouldDrainStop == true && record.IsNewDoc)
			{
				return false;
			}

			return true;
		}

		public override string GetDataTypeName(int i)
		{
			throw new NotImplementedException("IDataReader.GetDataTypeName should not be called on OpticonFileDataReader");
		}

		public override Type GetFieldType(int i)
		{
			throw new NotImplementedException("IDataReader.GetDataTypeName should not be called on OpticonFileDataReader");
		}
	}
}
