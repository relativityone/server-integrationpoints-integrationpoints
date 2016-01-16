using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTranfserDataReader : IDataReader
	{
		private readonly IRelativityClientAdaptor _relativityClientAdaptor;
		private readonly IEnumerable<int> _documentArtifactIds;
		private readonly DataTable _schemaDataTable;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private IEnumerator<Result<Document>> _documentsEnumerator;
		private Document _currentDocument;
		private bool _readerOpen;
		private IDictionary<int, string> _fieldIdToNameDictionary;

		public DocumentTranfserDataReader(IRelativityClientAdaptor relativityClientAdaptor, IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries)
		{
			_relativityClientAdaptor = relativityClientAdaptor;
			_documentArtifactIds = documentArtifactIds;
			_fieldEntries = fieldEntries.ToList();

			_readerOpen = true;
			_schemaDataTable = new DataTable();
			_schemaDataTable.Columns.AddRange(_fieldEntries.Select(x => new DataColumn(x.DisplayName)).ToArray());
			_fieldIdToNameDictionary = _fieldEntries.ToDictionary(x => Convert.ToInt32(x.FieldIdentifier), y => y.DisplayName);
			// TODO: we may need to specify the type
		}

		public void Close()
		{
			_readerOpen = false;
		}

		public int Depth
		{
			//change if we support nesting in the future
			get { return 0;} 
		}

		public DataTable GetSchemaTable()
		{
			return _schemaDataTable;
		}

		public bool IsClosed
		{
			get { return !_readerOpen; }
		}

		public bool NextResult()
		{
			return false; // This data reader only ever returns one set of data
		}

		public bool Read()
		{
			// if the reader is closed, go no further
			if (!_readerOpen) return false;

			// Check if search results have been populated
			if (_documentsEnumerator == null)
			{
				// Request document objects
				var artifactIdSetCondition = new WholeNumberCondition
				{
					Field = "ArtifactId",
					Operator = NumericConditionEnum.In,
					Value = _documentArtifactIds.ToList()
				};

				List<FieldValue> requestedFields = _fieldEntries.Select(x => new FieldValue() { ArtifactID = Convert.ToInt32(x.FieldIdentifier) }).ToList();

				Query<Document> query = new Query<Document>
				{
					Condition = artifactIdSetCondition,
					Fields = requestedFields
				};

				try
				{
					ResultSet<Document> docResults = _relativityClientAdaptor.ExecuteDocumentQuery(query);
					if (!docResults.Success)
					{
						_readerOpen = false; // TODO: handle errors?
					}
					else
					{
						_documentsEnumerator = docResults.Results.GetEnumerator();
					}
				}
				catch (Exception ex)
				{
					// TODO: Handle errors -- biedrzycki: Jan 13, 2015
					_readerOpen = false;
				}
			}

			// Get next result
			if (_documentsEnumerator != null && _documentsEnumerator.MoveNext())
			{
				Result<Document> result = _documentsEnumerator.Current;
				_currentDocument = result != null ? result.Artifact : null;

				_readerOpen = _currentDocument != null;
			}
			else
			{
				// No results returned, close the reader
				_currentDocument = null;
				_readerOpen = false;
			}

			return _readerOpen;
		}

		public int RecordsAffected
		{
			// this feature if wanted can be easily added just was not at this point because we are not supporting batching at this point
			get { return -1; }
		}

		public void Dispose()
		{
			_readerOpen = false;
			if (_documentsEnumerator != null)
			{
				_documentsEnumerator.Dispose();
			}
		}

		public int FieldCount
		{
			get { return 1; }
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
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			// This is used to expose nested tables and other hierarchical data but currently this is not desired
			throw new System.NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			return _schemaDataTable.Columns[i].DataType.Name;
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
			// TODO: double check this, this doesn't seem right
			return  _schemaDataTable.Columns[i].DataType;
		}

		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		public System.Guid GetGuid(int i)
		{
			return Guid.Parse(GetString(i));
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
			return _schemaDataTable.Columns[i].ColumnName;
		}

		public int GetOrdinal(string name)
		{
			return _schemaDataTable.Columns[name].Ordinal;
		}

		public string GetString(int i)
		{
			return Convert.ToString(GetValue(i));
		}

		public object GetValue(int i)
		{
			string fieldIdAsString = GetName(i);
			int fieldId = Convert.ToInt32(fieldIdAsString);
			string columnName = _fieldIdToNameDictionary[fieldId];
			return _currentDocument[columnName];
		}

		public int GetValues(object[] values)
		{
			// TODO: check if we need this
			throw new System.NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			return (GetValue(i) is System.DBNull);
		}

		public object this[string name]
		{
			get
			{
				return GetValue(GetOrdinal(name));
			}
		}

		public object this[int i]
		{
			get { return GetValue(i); }
		}
	}
}