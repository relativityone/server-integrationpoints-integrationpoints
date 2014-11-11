using System;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class JsonDataReader : IDataReader
	{
		private JArray _obj;
		private readonly string _filePath;
		private int _index ;

		public JsonDataReader(string filePath)
		{
			_filePath = filePath;
			_index = -1;
		}
		public void Dispose()
		{
			_obj = null;
		}

		public string GetName(int i)
		{
			var col = _obj.Children<JObject>().First().Properties().Select(x => x.Name).ToList()[i];
			return col;
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		public object GetValue(int i)
		{
			return _obj[_index].Values().ToList()[i].Value<string>();
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public int GetOrdinal(string name)
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
			return Convert.ToInt32(this.GetValue(i));
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
			return this.GetValue(i) as string;
		}

		public decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(this.GetValue(i));
		}

		public DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(this.GetValue(i));
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		public int FieldCount
		{
			get
			{
				if (_obj == null)
				{
					this.Read();
				}
				var cols = _obj.Children<JObject>().First().Properties().Select(x => x.Name).ToList();
				return cols.Count();
			}
		}

		object IDataRecord.this[int i]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		object IDataRecord.this[string name]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void Close()
		{
			
		}

		public DataTable GetSchemaTable()
		{
			if (_obj == null)
			{
				this.Read();
			}
			var columns = _obj.Children<JObject>().First().Properties().Select(x => new DataColumn(x.Name, typeof(string))).ToList();
			var dt = new DataTable();
			dt.Columns.AddRange(columns.ToArray());
			return dt;
		}

		public bool NextResult()
		{
			return Read();
		}

		public bool Read()
		{
			if (_obj == null)
			{
				using (StreamReader r = new StreamReader(_filePath))
				{
					string json = r.ReadToEnd();
					_obj = JsonConvert.DeserializeObject<JArray>(json);
				}
			}
			else
			{
				_index++;
			}
			return _obj.Count > _index;
		}

		public int Depth
		{
			get
			{
				return 1;
			}
		}

		public bool IsClosed { get; private set; }

		public int RecordsAffected
		{
			get
			{
				return 1;
			}
		}
	}
}
