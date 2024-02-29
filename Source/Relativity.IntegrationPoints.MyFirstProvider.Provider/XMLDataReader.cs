using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider
{
	public class XMLDataReader : IDataReader
	{
		private readonly DataTable _dataTable;
		private bool _readerOpen;
		private int _position = 0;
		private readonly IEnumerator<string> _itemsEnumerator;
		private readonly List<string> _fields;
		private readonly string _xmlFilePath;
		private XmlDocument _xmlDocument;
		private XmlNode _currentDataNode;
		private readonly string _keyFieldName;

		public XMLDataReader(IEnumerable<string> itemIds, List<string> fields, string keyFieldName, string xmlFilePath)
		{
			_xmlFilePath = xmlFilePath;
			_readerOpen = true;
			_itemsEnumerator = itemIds.GetEnumerator();
			_fields = fields;

			_dataTable = new DataTable();
			_dataTable.Columns.AddRange(fields.Select(f => new DataColumn(f)).ToArray());
			_keyFieldName = keyFieldName;
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
			get { return _readerOpen; }
		}

		public bool NextResult()
		{
			return false;
		}

		public bool Read()
		{
			if (_readerOpen)
			{
				_readerOpen = _itemsEnumerator.MoveNext();
				if (_readerOpen)
				{
					if (_xmlDocument == null)
					{
						_xmlDocument = new XmlDocument();
						_xmlDocument.Load(_xmlFilePath);
					}
					string xpath = string.Format("/root/data/document[{0}='{1}']", _keyFieldName, _itemsEnumerator.Current);
					XmlNodeList nodes = _xmlDocument.DocumentElement.SelectNodes(xpath);
					_currentDataNode = nodes[0];
					_position++;
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
			_itemsEnumerator.Dispose();
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
			return GetValue(i) as string;
		}

		public object GetValue(int i)
		{
			return _currentDataNode.SelectSingleNode(_fields[i]).InnerText;
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
