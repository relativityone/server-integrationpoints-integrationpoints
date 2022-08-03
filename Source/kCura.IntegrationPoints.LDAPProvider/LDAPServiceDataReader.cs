using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPServiceDataReader : IDataReader
    {
        private readonly DataTable _dataTable;
        private bool _readerOpen;
        private int _position = 0;
        private readonly ILDAPService _ldapService;
        private readonly List<string> _fields;
        private readonly ILDAPDataFormatter _dataFormattter;
        private readonly IEnumerator<string> _entryIds;
        private readonly string _identifier;
        private SearchResult _currentItem;

        public LDAPServiceDataReader(ILDAPService ldapService, IEnumerable<string> entryIds, string identifier, List<string> fields, ILDAPDataFormatter dataFormattter)
        {
            _ldapService = ldapService;
            _entryIds = entryIds.GetEnumerator();
            _dataFormattter = dataFormattter;
            _readerOpen = true;
            _fields = fields;
            _identifier = identifier;

            _dataTable = new DataTable();
            _dataTable.Columns.AddRange(fields.Select(f => new DataColumn(f)).ToArray());
        }

        public void Close()
        {
            _readerOpen = false;
            //TODO: is there anything to do?
            return;
        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            return _dataTable;
        }

        public bool IsClosed
        {
            get { return !_readerOpen; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            _currentItem = null;
            if (_readerOpen)
            {
                //TODO: possible optimization to retrieve multiple items per single call
                while (_currentItem == null && _readerOpen)
                {
                    _readerOpen = _entryIds.MoveNext();
                    if (_readerOpen)
                    {
                        _position++;
                        string filter = string.Format("({0}={1})", _identifier, _entryIds.Current);
                        IEnumerable<SearchResult> items = _ldapService.FetchItemsUpTheTree(filter, null);
                        if (items != null && items.Count() > 0)
                        {
                            _currentItem = items.First();
                        }
                    }
                }
            }
            return _readerOpen;
        }

        public int RecordsAffected
        {
            get { return _position; }
        }

        public void Dispose()
        {
            _readerOpen = false;
            _dataTable.Dispose();
            _entryIds.Dispose();
        }

        public int FieldCount
        {
            get { return _dataTable.Columns.Count; }
        }

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
            throw new System.NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(GetValue(i));
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new System.NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new System.NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return _dataTable.Columns[i].DataType.Name;
        }

        public System.DateTime GetDateTime(int i)
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

        public System.Type GetFieldType(int i)
        {
            return _dataTable.Columns[i].DataType;
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public System.Guid GetGuid(int i)
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
            return _dataTable.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return _dataTable.Columns[name].Ordinal;
        }

        public string GetString(int i)
        {
            return GetValue(i).ToString();
        }

        public object GetValue(int i)
        {
            return _dataFormattter.FormatData(_currentItem.Properties[_fields[i]]);
        }

        public int GetValues(object[] values)
        {
            if (values != null)
            {
                int fieldCount = Math.Min(values.Length, _fields.Count);
                object[] Values = new object[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    Values[i] = GetValue(i);
                }
                Array.Copy(Values, values, this.FieldCount);
                return fieldCount;
            }
            return 0;
        }

        public bool IsDBNull(int i)
        {
            return (GetValue(i) is System.DBNull);
        }

        public object this[string name]
        {
            get { return GetValue(_fields.IndexOf(name)); }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }
    }
}
