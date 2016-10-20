using System;
using System.Data;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFileDataReader : LoadFileBase, IDataReader
    {
        private bool _isClosed;
        private string _currentLine;
        private string _delimiterString;

        public LoadFileDataReader(kCura.WinEDDS.LoadFile config)
            : base(config)
        {
            _isClosed = false;
            _currentLine = string.Empty;
            _delimiterString = _config.RecordDelimiter.ToString();
        }

        public void Init()
        {
            //Accessing the ColumnNames is necessary to properly intialize the loadFileReader;
            //Otherwise ReadArtifact() throws "Object reference not set to an instance of an object."
            _loadFileReader.GetColumnNames(_config);

            //TODO: check settings object to decide whether to advance one record to skip headers
            this.Read();
        }

        //Get a line stored for the current row, based on delimter settings in the _config
        private void readCurrentRecord()
        {
            ArtifactFieldCollection artifacts = _loadFileReader.ReadArtifact();
            string[] data = new string[artifacts.Count];
            foreach (ArtifactField artifact in artifacts)
            {
                data[artifact.ArtifactID] = artifact.ValueAsString;
            }
            _currentLine = string.Join(_delimiterString, data);
        }

        //IDataReader Implementation

        public void Close()
        {
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
            throw new NotImplementedException();
        }

        public bool Read()
        {
            if (_loadFileReader.HasMoreRecords)
            {
                readCurrentRecord();
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
