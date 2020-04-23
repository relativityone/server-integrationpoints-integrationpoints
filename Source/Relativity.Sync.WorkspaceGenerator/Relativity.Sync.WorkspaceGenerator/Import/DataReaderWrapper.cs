using System;
using System.Collections.Generic;
using System.Data;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataReaderWrapper : IDataReader
	{
		private static readonly Type _typeOfString = typeof(string);

		private readonly IDocumentFactory _documentFactory;
		private readonly TestCase _testCase;

		private static IEnumerable<DataColumn> DefaultColumns => new[]
		{
			new DataColumn(ColumnNames.ControlNumber, typeof(string))
		};

		private static IEnumerable<DataColumn> NativesColumns => new[]
		{
			new DataColumn(ColumnNames.FileName, typeof(string)),
			new DataColumn(ColumnNames.NativeFilePath, typeof(string))
		};

		private static IEnumerable<DataColumn> ExtractedTextColumns => new[]
		{
			new DataColumn(ColumnNames.ExtractedText, typeof(string))
		};

		private int _currentDocumentIndex = 0;
		private Document _currentDocument;
		private DataTable _dataTable;
		private DataRow _currentRow;

		public DataReaderWrapper(IDocumentFactory documentFactory, TestCase testCase)
		{
			_documentFactory = documentFactory;
			_testCase = testCase;

			_dataTable = new DataTable();

			List<DataColumn> dataColumns = new List<DataColumn>(DefaultColumns);

			if (_testCase.GenerateNatives)
			{
				dataColumns.AddRange(NativesColumns);
			}

			if (_testCase.GenerateExtractedText)
			{
				dataColumns.AddRange(ExtractedTextColumns);
			}

			foreach (CustomField customField in _testCase.Fields)
			{
				dataColumns.Add(new DataColumn(customField.Name, typeof(string)));
			}

			_dataTable.Columns.AddRange(dataColumns.ToArray());
		}

		public bool Read()
		{
			_currentRow?.Delete();
			_currentDocument = _documentFactory.GetNextDocumentAsync().GetAwaiter().GetResult();

			if (_currentDocument == null)
			{
				return false;
			}

			Console.WriteLine($"Importing document ({++_currentDocumentIndex} of {_testCase.NumberOfDocuments}): {_currentDocument.Identifier}");

			_currentRow = _dataTable.NewRow();
			_currentRow[ColumnNames.ControlNumber] = _currentDocument.Identifier;

			if (_testCase.GenerateNatives)
			{
				_currentRow[ColumnNames.FileName] = _currentDocument.NativeFile.Name;
				_currentRow[ColumnNames.NativeFilePath] = _currentDocument.NativeFile.FullName;
			}

			if (_testCase.GenerateExtractedText)
			{
				_currentRow[ColumnNames.ExtractedText] = _currentDocument.ExtractedTextFile.FullName;
			}

			foreach (Tuple<string, string> customFieldValuePair in _currentDocument.CustomFields)
			{
				_currentRow[customFieldValuePair.Item1] = customFieldValuePair.Item2;
			}

			return true;
		}

		public string GetName(int i)
		{
			return _dataTable.Columns[i].ColumnName;
		}

		public int GetOrdinal(string name)
		{
			return _dataTable.Columns[name]?.Ordinal ??
			       throw new IndexOutOfRangeException($"The index of the {name} field wasn't found.");
		}

		public object GetValue(int i)
		{
			object value = _currentRow[i];

			if (value != null && GetFieldType(i) == _typeOfString)
			{
				return (value as string) ?? value.ToString();
			}

			return value;
		}

		public Type GetFieldType(int i)
		{
			return _dataTable.Columns[i].DataType;
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

		public bool IsClosed { get; private set; } = false;

		#region NotImplemented

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

		public int FieldCount => _dataTable.Columns.Count;

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		public int Depth { get; }

		public int RecordsAffected { get; }

		#endregion
	}
}