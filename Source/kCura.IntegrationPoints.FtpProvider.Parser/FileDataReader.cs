using System;
using System.Data;
using System.IO;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class FileDataReader : IDataReader
    {
        private StreamReader _file;
        private readonly string[] _headers;
        private string _currentLine;
        private int _currentIndex;

        public FileDataReader(string filePath)
        {
            _file = File.OpenText(filePath);
            Read();
            _headers = new string[1] { "ROW" };
        }

        public void Close()
        {
            this.Dispose();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { return _file == null; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            bool Eof = _file.EndOfStream;

            if (Eof)
            {
                return !Eof;
            }

            _currentLine = _file.ReadLine();
            if (_currentLine == null)
            {
                Eof = true;
            }
            else
            {
                _currentIndex++;
            }

            return !Eof;
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public int FieldCount
        {
            get { return _headers.Length; }
        }

        public bool GetBoolean(int i)
        {
            return Boolean.Parse(_currentLine);
        }

        public byte GetByte(int i)
        {
            return Byte.Parse(_currentLine);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Char.Parse(_currentLine);
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
            return _headers[i];
        }

        public int GetOrdinal(string name)
        {
            int result = -1;
            for (int i = 0; i < _headers.Length; i++)
            {
                if (_headers[i] == name)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        public string GetString(int i)
        {
            return _currentLine;
        }

        public object GetValue(int i)
        {
            return _currentLine;
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return string.IsNullOrWhiteSpace(_currentLine);
        }

        public object this[string name]
        {
            get { return _currentLine; }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        public void Dispose()
        {
            if (_file != null)
            {
                _file.Dispose();
                _file = null;
            }
        }
    }
}
