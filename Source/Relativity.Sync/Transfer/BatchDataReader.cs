﻿using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal class BatchDataReader : IDataReader
	{
		private readonly DataTable _templateDataTable;

		private readonly int _sourceWorkspaceArtifactId;
		private readonly RelativityObjectSlim[] _batch;
		private readonly IEnumerator<object[]> _batchEnumerator;

		private readonly IReadOnlyList<FieldInfoDto> _allFields;
		private readonly IFieldManager _fieldManager;
		private readonly IExportDataSanitizer _exportDataSanitizer;

		private readonly CancellationToken _cancellationToken;

		private static readonly Type _typeOfString = typeof(string);

		public object this[int i]
		{
			get { return GetValue(i); }
		}

		public object this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		public int Depth { get; } = 0;

		public bool IsClosed { get; private set; } = false;

		public int RecordsAffected { get; } = 0;

		public int FieldCount
		{
			get { return _templateDataTable.Columns.Count; }
		}

		public BatchDataReader(DataTable templateDataTable,
			int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch,
			IReadOnlyList<FieldInfoDto> allFields, IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer,
			CancellationToken cancellationToken)
		{
			_templateDataTable = templateDataTable;

			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_batch = batch;
			_batchEnumerator = GetBatchEnumerable().GetEnumerator();

			_allFields = allFields;
			_fieldManager = fieldManager;
			_exportDataSanitizer = exportDataSanitizer;

			_cancellationToken = cancellationToken;
		}

		public void Close()
		{
			IsClosed = true;
		}

		public void Dispose()
		{
			_batchEnumerator.Dispose();
		}

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
			return _templateDataTable.Columns[name].Ordinal;
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
			IsClosedCheck();

			return false;
		}

		public bool Read()
		{
			IsClosedCheck();

			if (_cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			return _batchEnumerator.MoveNext();
		}

		private void IsClosedCheck()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("The IDataReader is closed.");
			}
		}

		private IEnumerable<object[]> GetBatchEnumerable()
		{
			if (_batch != null && _batch.Any())
			{
				IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = CreateSpecialFieldRowValuesBuilders();

				foreach (RelativityObjectSlim item in _batch)
				{
					yield return BuildRow(specialFieldBuildersDictionary, item);
				}
			}
		}

		private IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> CreateSpecialFieldRowValuesBuilders()
		{
			// TODO REL-367580: [PERFORMANCE] It looks like we are creating this collection (Int32 x Batch Size) unnecessary.
			//                  We could pass IEnumerable further, but currently the whole stack is expecting ICollection so the change is to deep for this issue.
			ICollection<int> documentArtifactIds = _batch.Select(obj => obj.ArtifactID).ToList();

			return _fieldManager.CreateSpecialFieldRowValueBuildersAsync(_sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private object[] BuildRow(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem)
		{
			object[] result = new object[_allFields.Count];

			for (int i = 0; i < _allFields.Count; i++)
			{
				FieldInfoDto field = _allFields[i];
				if (field.SpecialFieldType != SpecialFieldType.None)
				{
					object specialValue = BuildSpecialFieldValue(specialFieldBuilders, batchItem, field);
					result[i] = specialValue;
				}
				else
				{
					FieldInfoDto identifierField = _fieldManager.GetObjectIdentifierFieldAsync(_cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
					string itemIdentifier = batchItem.Values[identifierField.DocumentFieldIndex].ToString();
					object initialValue = batchItem.Values[field.DocumentFieldIndex];
					result[i] = SanitizeFieldIfNeeded(identifierField.SourceFieldName, itemIdentifier, field, initialValue);
				}
			}

			return result;
		}

		private static object BuildSpecialFieldValue(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem, FieldInfoDto fieldInfo)
		{
			if (!specialFieldBuilders.ContainsKey(fieldInfo.SpecialFieldType))
			{
				throw new SourceDataReaderException($"No special field row value builder found for special field type {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType}");
			}
			object initialFieldValue = fieldInfo.IsDocumentField ? batchItem.Values[fieldInfo.DocumentFieldIndex] : null;

			return specialFieldBuilders[fieldInfo.SpecialFieldType].BuildRowValue(fieldInfo, batchItem, initialFieldValue);
		}

		private object SanitizeFieldIfNeeded(string itemIdentifierFieldName, string itemIdentifier, FieldInfoDto field, object initialValue)
		{
			object sanitizedValue = initialValue;
			if (_exportDataSanitizer.ShouldSanitize(field.RelativityDataType))
			{
				sanitizedValue = _exportDataSanitizer.SanitizeAsync(_sourceWorkspaceArtifactId, itemIdentifierFieldName, itemIdentifier, field, initialValue).ConfigureAwait(false).GetAwaiter().GetResult();
			}

			return sanitizedValue;
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

		public string GetString(int i)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
