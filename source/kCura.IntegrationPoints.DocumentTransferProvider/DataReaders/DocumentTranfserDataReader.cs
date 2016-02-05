using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
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
		private readonly IDictionary<string, string> _fieldIdToNameDictionary;

		private readonly HashSet<int> _longTextFieldsArtifactIds;

		public DocumentTranfserDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> documentFields)
		{
			_relativityClientAdaptor = relativityClientAdaptor;
			_documentArtifactIds = documentArtifactIds;
			_fieldEntries = fieldEntries.ToList();

			_readerOpen = true;
			_schemaDataTable = new DataTable();
			_schemaDataTable.Columns.AddRange(_fieldEntries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray());
			_fieldIdToNameDictionary = _fieldEntries.ToDictionary(
				x => x.FieldIdentifier,
				y => y.IsIdentifier ? y.DisplayName.Replace(Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, String.Empty) : y.DisplayName);

			_longTextFieldsArtifactIds =  new HashSet<int>(documentFields.Select(artifact => artifact.ArtifactID));
		}

		public void Close()
		{
			_readerOpen = false;
			if (_documentsEnumerator != null)
			{
				_documentsEnumerator.Dispose();
				_documentsEnumerator = null;
			}
			_currentDocument = null;
		}

		public int Depth
		{
			//change if we support nesting in the future
			get { return 0; }
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
					Field = Shared.Constants.ARTIFACT_ID_FIELD_NAME,
					Operator = NumericConditionEnum.In,
					Value = _documentArtifactIds.ToList()
				};

				// only query for non-full text fields at this time
				List<FieldValue> requestedFields = _fieldEntries.Where(field => _longTextFieldsArtifactIds.Contains(Convert.ToInt32(field.FieldIdentifier)) == false)
																.Select(x => new FieldValue() { ArtifactID = Convert.ToInt32(x.FieldIdentifier) }).ToList();
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

		// Following this example: https://msdn.microsoft.com/en-us/library/aa720693(v=vs.71).aspx -- biedrzycki: Jan 20th, 2016
		public void Dispose()
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					this.Close();
				}
				catch (Exception e)
				{
					throw new SystemException("An exception of type " + e.GetType() +
											  " was encountered while closing the DocumentTransferDataReader.");
				}
			}
		}

		public int FieldCount
		{
			get { return _schemaDataTable.Columns.Count; }
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
			return _schemaDataTable.Columns[i].DataType;
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

			Object result = null;
			if (_longTextFieldsArtifactIds.Contains(fieldId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldId);
			}
			else
			{
				string columnName = _fieldIdToNameDictionary[fieldIdAsString].Replace(Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, String.Empty);
				result = _currentDocument[columnName].Value;
			}
			return result;
		}

		public int GetValues(object[] values)
		{
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

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldValue(_currentDocument.ArtifactID, fieldArtifactId);
		}

		private String GetLongTextFieldValue(int documentArtifactId, int longTextFieldArtifactId)
		{
			Document documentQuery = new Document(documentArtifactId)
			{
				Fields = new List<FieldValue>() { new FieldValue(longTextFieldArtifactId) }
			};

			ResultSet<Document> results = null;
			try
			{
				results = _relativityClientAdaptor.ReadDocument(documentQuery);
			}
			catch (Exception e)
			{
				const string exceptionMessage = "Unable to read document of artifact id {0}. This may be due to the size of the field. Please reconfigure Relativity.Services' web.config to resolve the issue.";
				throw new ProviderReadDataException(String.Format(exceptionMessage, documentArtifactId), e)
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			var document = results.Results.FirstOrDefault();
			if (results.Success == false || document == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a document object with artifact Id of {0}", documentArtifactId))
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			Document documentArtifact = document.Artifact;
			var extractedText = documentArtifact.Fields.FirstOrDefault();
			if (extractedText == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a long field with artifact Id of {0}", longTextFieldArtifactId))
				{
					Identifier = documentArtifactId.ToString()
				};
			}
			return extractedText.ValueAsLongText;
		}
	}
}