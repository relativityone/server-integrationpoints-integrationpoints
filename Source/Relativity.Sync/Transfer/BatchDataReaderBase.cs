using Relativity.API;
using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal abstract class BatchDataReaderBase : IBatchDataReader
	{
		private FieldInfoDto _identifierField;

		private readonly DataTable _templateDataTable;
		private readonly IEnumerator<object[]> _batchEnumerator;

		protected readonly int _sourceWorkspaceArtifactId;
		protected readonly RelativityObjectSlim[] _batch;
		protected readonly IReadOnlyList<FieldInfoDto> _allFields;
		protected readonly IFieldManager _fieldManager;
		protected readonly IExportDataSanitizer _exportDataSanitizer;
		protected readonly Action<string, string> _itemLevelErrorHandler;
		protected readonly CancellationToken _cancellationToken;
		protected readonly IAPILog _logger;

		protected static readonly Type _typeOfString = typeof(string);

		protected FieldInfoDto IdentifierField
		{
			get
			{
				if (_identifierField is null)
				{
					_identifierField = _fieldManager.GetObjectIdentifierFieldAsync(_cancellationToken).GetAwaiter().GetResult();
				}

				return _identifierField;
			}
		}

		public object this[int i] => GetValue(i);

		public object this[string name] => GetValue(GetOrdinal(name));

		public int Depth { get; } = 0;

		public bool IsClosed { get; private set; } = false;

		public int RecordsAffected { get; } = 0;

		public int FieldCount => _templateDataTable.Columns.Count;

		protected BatchDataReaderBase(
			DataTable templateDataTable,
			int sourceWorkspaceArtifactId,
			RelativityObjectSlim[] batch,
			IReadOnlyList<FieldInfoDto> allFields,
			IFieldManager fieldManager,
			IExportDataSanitizer exportDataSanitizer,
			Action<string, string> itemLevelErrorHandler,
			CancellationToken cancellationToken,
			IAPILog logger)
		{
			_templateDataTable = templateDataTable;

			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_batch = batch;

			_allFields = allFields;
			_fieldManager = fieldManager;
			_exportDataSanitizer = exportDataSanitizer;

			_itemLevelErrorHandler = itemLevelErrorHandler;

			_cancellationToken = cancellationToken;
			_logger = logger;

			_batchEnumerator = GetBatchEnumerable().GetEnumerator();
		}

		protected abstract IEnumerable<object[]> GetBatchEnumerable();

		public bool CanCancel { get; protected set; }

		public string GetDataTypeName(int i)
		{
			return _templateDataTable.Columns[i].DataType.Name;
		}

		public Type GetFieldType(int i)
		{
			return _templateDataTable.Columns[i].DataType;
		}

		public string GetName(int i)
		{
			return _templateDataTable.Columns[i].ColumnName;
		}

		public int GetOrdinal(string name)
		{
			return _templateDataTable.Columns[name]?.Ordinal ?? throw new IndexOutOfRangeException($"The index of the {name} field wasn't found.");
		}

		public DataTable GetSchemaTable()
		{
			return _templateDataTable.Clone();
		}

		public object GetValue(int i)
		{
			object value = _batchEnumerator.Current[i];

			if (value != null && GetFieldType(i) == _typeOfString)
			{
				return (value as string) ?? value.ToString();
			}

			return value;
		}

		public string GetString(int i)
		{
			return GetValue(i).ToString();
		}

		public int GetValues(object[] values)
		{
			int columnIndex = 0;

			for (; columnIndex < values.Length && columnIndex < _batchEnumerator.Current.Length; columnIndex++)
			{
				values[columnIndex] = _batchEnumerator.Current[columnIndex];
			}

			return columnIndex;
		}

		public bool IsDBNull(int i)
		{
			return _batchEnumerator.Current[i] is null;
		}

		public bool NextResult()
		{
			ThrowIfIsClosed();

			return false;
		}

		public bool Read()
		{
			ThrowIfIsClosed();
			
			return _batchEnumerator.MoveNext();
		}

		private void ThrowIfIsClosed()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("The IDataReader is closed.");
			}
		}

		public void Close()
		{
			IsClosed = true;
		}

		public void Dispose()
		{
			_batchEnumerator.Dispose();
		}

		#region Not Implemented
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
		#endregion
	}
}
