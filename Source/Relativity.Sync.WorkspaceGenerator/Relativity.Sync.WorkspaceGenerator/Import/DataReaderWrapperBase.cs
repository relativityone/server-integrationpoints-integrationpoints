using System;
using System.Data;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public abstract class DataReaderWrapperBase : IDataReaderWrapper
    {
        private static readonly Type _typeOfString = typeof(string);
        protected readonly IDocumentFactory DocumentFactory;
        protected readonly TestCase TestCase;
        protected readonly DataTable DataTable;
        protected readonly int BatchSize;
        protected readonly int AlreadyProvidedRecordCount;
        protected int CurrentDocumentIndex = 0;
        protected Document CurrentDocument;
        protected DataRow CurrentRow;

        public DataReaderWrapperBase(IDocumentFactory documentFactory, TestCase testCase, int batchSize, int alreadyProvidedRecordCount)
        {
            DocumentFactory = documentFactory;
            TestCase = testCase;
            BatchSize = batchSize;
            AlreadyProvidedRecordCount = alreadyProvidedRecordCount;

            DataTable = new DataTable();
        }

        public bool IsClosed { get; private set; } = false;
        public int FieldCount => DataTable.Columns.Count;
        public int Depth { get; }
        public int RecordsAffected { get; }

        public DataTable ReadToSimpleDataTable()
        {
            var table = DataTable.Copy();

            while (Read())
            {
                var newRow = table.NewRow();
                newRow.ItemArray =(object[]) CurrentRow.ItemArray.Clone();
                table.Rows.Add(newRow);
            }

            return table;
        }

        public abstract bool Read();

        public string GetName(int i)
        {
            return DataTable.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return DataTable.Columns[name]?.Ordinal ??
                   throw new IndexOutOfRangeException($"The index of the {name} field wasn't found.");
        }

        public object GetValue(int i)
        {
            object value = CurrentRow[i];

            if (value != null && GetFieldType(i) == _typeOfString)
            {
                return (value as string) ?? value.ToString();
            }

            return value;
        }

        public Type GetFieldType(int i)
        {
            return DataTable.Columns[i].DataType;
        }

        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));

        public void Close()
        {
            IsClosed = true;
        }

        public void Dispose()
        {
            Close();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
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

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw  new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }
    }
}