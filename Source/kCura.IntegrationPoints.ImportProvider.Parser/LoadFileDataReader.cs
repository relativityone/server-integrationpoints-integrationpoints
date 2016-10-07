using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;


using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFileDataReader : LoadFileBase, IDataReader
    {
        private bool _isClosed;
        private string _currentLine;

        public LoadFileDataReader(kCura.WinEDDS.LoadFile config)
            : base(config)
        {
            SeqLogger.Info("LoadFileDataReader ctor start");
            _isClosed = false;
            _currentLine = string.Empty;

            //Accessing the ColumnNames is necessary to properly intialize the loadFileReader;
            //Otherwise ReadArtifact() throws "Object reference not set to an instance of an object."
            SeqLogger.Info("LoadFileDataReader ctor about to call GetColumnNames... ");
            _loadFileReader.GetColumnNames(_config);

            //TODO: check settings object to decide whether to advance one record to skip headers
            this.Read();
            SeqLogger.Info("LoadFileDataReader ctor done.");
        }

        //Get a line stored for the current row, based on delimter settings in the _config
        private void readCurrentRecord()
        {
            var fields = new List<string>();
            //TODO: dont assume CSV, do string join based on _config settings
            //TODO: pull the list of selected fields (either from options string, for do an RDO lookup) to only put mapped fields into the _currentLine string

            SeqLogger.Info("readCurrentRecord about to read artifacts.");
            foreach (var artifact in _loadFileReader.ReadArtifact())
            {
                SeqLogger.Info("Artifact: {DisplayName}: {Artifact}", artifact.DisplayName, artifact.ValueAsString);
                fields.Add(artifact.ValueAsString);
            }
            _currentLine = string.Join(",", fields);
            SeqLogger.Info("**readCurrentRecord setting current to {Current}", _currentLine);
        }

        //IDataReader Implementation

        public void Close()
        {
            SeqLogger.Info("LoadFileDataReader.Close()");
            _loadFileReader.Close();
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
            throw new NotImplementedException("DataReader.NextResult() not implemented.");
        }

        public bool Read()
        {
            SeqLogger.Info("LoadFileDataReader.Read()");
            SeqLogger.Info("_loadFileReader.HasMoreRecords: {Bool}", _loadFileReader.HasMoreRecords);

            if (!_loadFileReader.HasMoreRecords)
            {
                SeqLogger.Info("LoadFileDataReader.Read() returning False");
                return false;
            }
            else
            {
                SeqLogger.Info("LoadFileDataReader.Read() returning True");
                readCurrentRecord();
                return true;
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
            SeqLogger.Info("LoadFileDataReader.GetString({i})", i);
            SeqLogger.Info("LoadFileDataReader.GetString returning {String}", _currentLine);
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
