using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPDataReader : IDataReader
    {
        public bool IsClosed { get; private set; }
        public int RecordsAffected { get; private set; }

        private readonly DataTable _schemaTable;
        private readonly IEnumerator<SearchResult> _itemsEnumerator;
        private readonly ILDAPDataFormatter _dataFormatter;

        public LDAPDataReader(IEnumerable<SearchResult> items, List<string> fields, ILDAPDataFormatter dataFormatter)
        {
            _dataFormatter = dataFormatter;
            IsClosed = false;
            _itemsEnumerator = items.GetEnumerator();

            _schemaTable = new DataTable();
            _schemaTable.Columns.AddRange(fields.Select(f => new DataColumn(f)).ToArray());
        }

        public void Close()
        {
            IsClosed = true;
            //TODO: is there anything to do?
        }

        public int Depth => 0;

        public DataTable GetSchemaTable()
        {
            return _schemaTable;
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (!IsClosed)
            {
                IsClosed = !_itemsEnumerator.MoveNext();
                if (!IsClosed)
                {
                    RecordsAffected++;
                }
            }
            return !IsClosed;
        }

        public void Dispose()
        {
            IsClosed = true;
            _schemaTable.Dispose();
            _itemsEnumerator.Dispose();
        }

        public int FieldCount => _schemaTable.Columns.Count;

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(GetValue(i));
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(GetValue(i));
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(GetValue(i));
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
            return _schemaTable.Columns[i].DataType.Name;
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(GetValue(i));
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(GetValue(i));
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(GetValue(i));
        }

        public Type GetFieldType(int i)
        {
            return _schemaTable.Columns[i].DataType;
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(GetValue(i).ToString());
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(GetValue(i));
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(GetValue(i));
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(GetValue(i));
        }

        public string GetName(int i)
        {
            return _schemaTable.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return _schemaTable.Columns[name].Ordinal;
        }

        public string GetString(int i)
        {
            return GetValue(i) as string;
        }

        public object GetValue(int i)
        {
            // TODO Shouldn't this method return actual value instead of formatted string?
            return _dataFormatter.FormatData(_itemsEnumerator.Current.Properties[GetName(i)]);
        }

        public int GetValues(object[] values)
        {
            if (values == null)
            {
                return 0;
            }

            int fieldCount = Math.Min(values.Length, FieldCount);
            var newValues = new object[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                newValues[i] = GetValue(i);
            }
            Array.Copy(newValues, values, fieldCount);
            return fieldCount;
        }
        
        // TODO FIXME This method is currently useless, as formatter in GetValue always returns string
        public bool IsDBNull(int i)
        {
            return GetValue(i) is DBNull;
        }

        public object this[string name] => GetValue(GetOrdinal(name));

        public object this[int i] => GetValue(i);
    }
}