using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Microsoft.VisualBasic.FileIO;

[assembly: InternalsVisibleTo("kCura.IntegrationPoints.FtpProvider.Parser.Tests")]

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class DelimitedFileParser : IDataReader, IParser
    {
        internal bool _disposed;
        internal TextFieldParser _parser;
        internal Stream _fileStream;
        internal TextReader _textReader;

        internal String _fileLocation;
        internal String[] _columns;
        internal String[] _currentLine;
        internal Int32 _lineNumber = 0;

        public int RecordsAffected
        {
            get { return _lineNumber; }
        }
        public bool IsClosed
        {
            get { return _disposed; }
        }

        public DelimitedFileParser(string fileLocation, ParserOptions parserOptions)
        {
            _fileLocation = fileLocation;
            if (SourceExists())
            {
                _parser = new TextFieldParser(_fileLocation);
                SetParserOptions(_parser, parserOptions);
            }

            if (parserOptions.FirstLineContainsColumnNames)
            {
                ParseColumns();
            }
        }

        public DelimitedFileParser(Stream stream, ParserOptions parserOptions)
        {
            _fileStream = stream;
            if (SourceExists())
            {
                _parser = new TextFieldParser(_fileStream, Encoding.UTF8, true);
                SetParserOptions(_parser, parserOptions);
            }
            if (parserOptions.FirstLineContainsColumnNames)
            {
                ParseColumns();
            }
        }

        public DelimitedFileParser(TextReader reader, ParserOptions parserOptions, List<string> columnList)
        {
            _textReader = reader;
            if (SourceExists())
            {
                _parser = new TextFieldParser(_textReader);
                SetParserOptions(_parser, parserOptions);
            }
            _columns = columnList.ToArray();
        }

        private void SetParserOptions(TextFieldParser parser, ParserOptions parserOptions)
        {
            parser.TextFieldType = parserOptions.TextFieldType;
            parser.Delimiters = parserOptions.Delimiters;
            parser.HasFieldsEnclosedInQuotes = parserOptions.HasFieldsEnclosedInQuotes;
            parser.TrimWhiteSpace = parserOptions.HasFieldsEnclosedInQuotes;
        }

        public Boolean SourceExists()
        {
            if ((_fileLocation != null && !new FileInfo(_fileLocation).Exists) && _fileStream == null && _textReader == null)
            {
                throw new Exceptions.CantAccessSourceException();
            }
            return true;
        }

        public IEnumerable<String> ParseColumns()
        {
            String[] retVal = null;
            if (SourceExists())
            {
                if (_columns != null)
                {
                    retVal = _columns;
                }
                else
                {
                    if (NextResult())
                    {
                        _columns = _currentLine;
                        //Make sure headers exist in file
                        if (_columns == null || _columns.Length < 1)
                        {
                            throw new Exceptions.NoColumnsExcepetion();
                        }
                        ValidateColumns(_columns);
                        retVal = _columns;
                    }
                    else
                    {
                        throw new Exceptions.NoColumnsExcepetion();
                    }
                }
            }
            return retVal;
        }

        internal void ValidateColumns(IEnumerable<String> columns)
        {
            //Validate Blank Columns
            foreach (var column in columns)
            {
                if (String.IsNullOrWhiteSpace(column))
                {
                    throw new Exceptions.BlankColumnExcepetion();
                }
            }

            //Validate Duplicates
            var destination = new List<String>();
            foreach (var column in columns)
            {
                if (!destination.Contains(column))
                {
                    destination.Add(column);
                }
                else
                {
                    throw new Exceptions.DuplicateColumnsExistExcepetion();
                }
            }
        }

        public IDataReader ParseData()
        {
            return this;
        }

        public int Depth
        {
            get { return 1; }
        }

        public bool Read()
        {
            return NextResult();
        }

        public bool NextResult()
        {
            bool retVal = false;
            if (!_parser.EndOfData)
            {
                _lineNumber++;
                string[] data = _parser.ReadFields();
                if (data != null)
                {
                    if (_columns != null && data.Length != _columns.Length)
                    {
                        throw new Exceptions.NumberOfColumnsNotEqualToNumberOfDataValuesException(_lineNumber);
                    }
                    _currentLine = data;
                    retVal = true;
                }
            }
            return retVal;
        }

        public void Close()
        {
            Dispose();
        }

        public DataTable GetSchemaTable()
        {
            DataTable t = new DataTable();
            t.Columns.Add("Name");
            for (int i = 0; i < _columns.Length; i++)
            {
                DataRow row = t.NewRow();
                row["Name"] = _columns[i];
                t.Rows.Add(row);
            }
            return t;
        }

        public int FieldCount
        {
            get { return _columns.Length; }
        }

        public bool GetBoolean(int i)
        {
            return Boolean.Parse(_currentLine[i]);
        }

        public byte GetByte(int i)
        {
            return Byte.Parse(_currentLine[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Char.Parse(_currentLine[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return (IDataReader)this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return DateTime.Parse(_currentLine[i]);
        }

        public decimal GetDecimal(int i)
        {
            return Decimal.Parse(_currentLine[i]);
        }

        public double GetDouble(int i)
        {
            return Double.Parse(_currentLine[i]);
        }

        public Type GetFieldType(int i)
        {
            return typeof(String);
        }

        public float GetFloat(int i)
        {
            return float.Parse(_currentLine[i]);
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(_currentLine[i]);
        }

        public short GetInt16(int i)
        {
            return Int16.Parse(_currentLine[i]);
        }

        public int GetInt32(int i)
        {
            return Int32.Parse(_currentLine[i]);
        }

        public long GetInt64(int i)
        {
            return Int64.Parse(_currentLine[i]);
        }

        public string GetName(int i)
        {
            return _columns[i];
        }

        public int GetOrdinal(string name)
        {
            int result = -1;
            for (int i = 0; i < _columns.Length; i++)
                if (_columns[i] == name)
                {
                    result = i;
                    break;
                }
            return result;
        }

        public string GetString(int i)
        {
            return _currentLine[i];
        }

        public object GetValue(int i)
        {
            return _currentLine[i];
        }

        public int GetValues(object[] values)
        {
            for (var i = 0; i < _currentLine.Length; i++)
            {
                values[i] = _currentLine[i];
            }
            return 1;
        }

        public bool IsDBNull(int i)
        {
            return string.IsNullOrWhiteSpace(_currentLine[i]);
        }

        public object this[string name]
        {
            get { return _currentLine[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_parser != null)
                    {
                        _parser.Dispose();
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}