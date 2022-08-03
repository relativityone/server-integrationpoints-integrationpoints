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
        internal bool Disposed;
        internal TextFieldParser Parser;
        internal Stream FileStream;
        internal TextReader TextReader;

        internal string FileLocation;
        internal string[] Columns;
        internal string[] CurrentLine;
        internal int LineNumber = 0;

        public int RecordsAffected => LineNumber;

        public bool IsClosed => Disposed;

        public DelimitedFileParser(string fileLocation, ParserOptions parserOptions)
        {
            FileLocation = fileLocation;
            if (SourceExists())
            {
                Parser = new TextFieldParser(FileLocation);
                SetParserOptions(Parser, parserOptions);
            }

            if (parserOptions.FirstLineContainsColumnNames)
            {
                ParseColumns();
            }
        }

        public DelimitedFileParser(Stream stream, ParserOptions parserOptions)
        {
            FileStream = stream;
            if (SourceExists())
            {
                Parser = new TextFieldParser(FileStream, Encoding.UTF8, true);
                SetParserOptions(Parser, parserOptions);
            }
            if (parserOptions.FirstLineContainsColumnNames)
            {
                ParseColumns();
            }
        }

        //this one is not tested
        public DelimitedFileParser(TextReader reader, ParserOptions parserOptions, List<string> columnList)
        {
            TextReader = reader;
            if (SourceExists())
            {
                Parser = new TextFieldParser(TextReader);
                SetParserOptions(Parser, parserOptions);
            }
            Columns = columnList.ToArray();
        }

        private void SetParserOptions(TextFieldParser parser, ParserOptions parserOptions)
        {
            parser.TextFieldType = parserOptions.TextFieldType;
            parser.Delimiters = parserOptions.Delimiters;
            parser.HasFieldsEnclosedInQuotes = parserOptions.HasFieldsEnclosedInQuotes;
            parser.TrimWhiteSpace = parserOptions.HasFieldsEnclosedInQuotes;
        }

        public bool SourceExists()
        {
            if ((FileLocation != null && !new FileInfo(FileLocation).Exists) && FileStream == null && TextReader == null)
            {
                throw new Exceptions.CantAccessSourceException();
            }
            return true;
        }

        public IEnumerable<string> ParseColumns()
        {
            string[] retVal = null;
            if (SourceExists())
            {
                if (Columns != null)
                {
                    retVal = Columns;
                }
                else
                {
                    if (NextResult())
                    {
                        Columns = CurrentLine;
                        //Make sure headers exist in file
                        if (Columns == null || Columns.Length < 1)
                        {
                            throw new Exceptions.NoColumnsException();
                        }
                        ValidateColumns(Columns);
                        retVal = Columns;
                    }
                    else
                    {
                        throw new Exceptions.NoColumnsException();
                    }
                }
            }
            return retVal;
        }

        internal void ValidateColumns(IEnumerable<string> columns)
        {
            //Validate Blank Columns
            foreach (string column in columns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    throw new Exceptions.BlankColumnException();
                }
            }

            //Validate Duplicates
            var destination = new List<string>();
            foreach (string column in columns)
            {
                if (!destination.Contains(column))
                {
                    destination.Add(column);
                }
                else
                {
                    throw new Exceptions.DuplicateColumnsExistException();
                }
            }
        }

        public IDataReader ParseData()
        {
            return this;
        }

        public int Depth => 1;

        public bool Read()
        {
            return NextResult();
        }

        public bool NextResult()
        {
            var retVal = false;
            if (!Parser.EndOfData)
            {
                LineNumber++;
                string[] data = Parser.ReadFields();
                if (data != null)
                {
                    if (Columns != null && data.Length != Columns.Length)
                    {
                        throw new Exceptions.NumberOfColumnsNotEqualToNumberOfDataValuesException(LineNumber);
                    }
                    CurrentLine = data;
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
            var t = new DataTable();
            t.Columns.Add("Name");
            for (var i = 0; i < Columns.Length; i++)
            {
                DataRow row = t.NewRow();
                row["Name"] = Columns[i];
                t.Rows.Add(row);
            }
            return t;
        }

        public int FieldCount => Columns.Length;

        public bool GetBoolean(int i)
        {
            return bool.Parse(CurrentLine[i]);
        }

        public byte GetByte(int i)
        {
            return byte.Parse(CurrentLine[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public char GetChar(int i)
        {
            return char.Parse(CurrentLine[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotSupportedException();
        }

        public DateTime GetDateTime(int i)
        {
            return DateTime.Parse(CurrentLine[i]);
        }

        public decimal GetDecimal(int i)
        {
            return decimal.Parse(CurrentLine[i]);
        }

        public double GetDouble(int i)
        {
            return double.Parse(CurrentLine[i]);
        }

        public Type GetFieldType(int i)
        {
            return (CurrentLine[i]).GetType();
        }

        public float GetFloat(int i)
        {
            return float.Parse(CurrentLine[i]);
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(CurrentLine[i]);
        }

        public short GetInt16(int i)
        {
            return short.Parse(CurrentLine[i]);
        }

        public int GetInt32(int i)
        {
            return int.Parse(CurrentLine[i]);
        }

        public long GetInt64(int i)
        {
            return long.Parse(CurrentLine[i]);
        }

        public string GetName(int i)
        {
            return Columns[i];
        }

        public int GetOrdinal(string name)
        {
            int result = -1;
            for (var i = 0; i < Columns.Length; i++)
            {
                if (Columns[i] == name)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public string GetString(int i)
        {
            return CurrentLine[i];
        }

        public object GetValue(int i)
        {
            return CurrentLine[i];
        }

        public int GetValues(object[] values)
        {
            for (var i = 0; i < CurrentLine.Length; i++)
            {
                values[i] = CurrentLine[i];
            }
            return 1;
        }

        public bool IsDBNull(int i)
        {
            return string.IsNullOrWhiteSpace(CurrentLine[i]);
        }

        public object this[string name] => CurrentLine[GetOrdinal(name)];

        public object this[int i] => GetValue(i);

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Parser?.Dispose();
                }
            }
            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}