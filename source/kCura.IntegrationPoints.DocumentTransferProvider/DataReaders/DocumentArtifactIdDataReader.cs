using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentArtifactIdDataReader : IDataReader
	{
		private readonly IRelativityClientAdaptor _relativityClient;
		private readonly int _savedSearchArtifactId;
		private IEnumerator<Result<Document>> _savedSearchResultDocumentsEnumerator;
		private Document _currentDocument;
		private bool _readerOpen;

		public DocumentArtifactIdDataReader(IRelativityClientAdaptor relativityClientAdaptor, int savedSearchArtifactId)
		{
			_relativityClient = relativityClientAdaptor;
			_savedSearchArtifactId = savedSearchArtifactId;

			_readerOpen = true;
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
			throw new NotImplementedException();
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
			if (_savedSearchResultDocumentsEnumerator == null)
			{
				// Request the saved search documents
				Query<Document> query = new Query<Document>
				{
					Condition = new SavedSearchCondition(_savedSearchArtifactId),
					Fields = FieldValue.NoFields // we only want the ArtifactId
				};

				try
				{
					ResultSet<Document> docResults = _relativityClient.ExecuteDocumentQuery(query);
					if (!docResults.Success)
					{
						_readerOpen = false; // TODO: handle errors?
					}
					else
					{
						_savedSearchResultDocumentsEnumerator = docResults.Results.GetEnumerator();
					}
				}
				catch (Exception ex)
				{
					// TODO: Handle errors -- biedrzycki: Jan 13, 2015
					_readerOpen = false;
				}
			}

			// Get next result
			if (_savedSearchResultDocumentsEnumerator != null && _savedSearchResultDocumentsEnumerator.MoveNext())
			{
				Result<Document> result = _savedSearchResultDocumentsEnumerator.Current;
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
			if (_savedSearchResultDocumentsEnumerator != null)
			{
				_savedSearchResultDocumentsEnumerator.Dispose();
			}
		}

		public int FieldCount
		{
			get { return 1; }
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
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public char GetChar(int i)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public System.DateTime GetDateTime(int i)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			throw new System.NotImplementedException();
		}

		public double GetDouble(int i)
		{
			throw new System.NotImplementedException();
		}

		public System.Type GetFieldType(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}

			return typeof (Int32);
		}

		public float GetFloat(int i)
		{
			throw new System.NotImplementedException();
		}

		public System.Guid GetGuid(int i)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public short GetInt16(int i)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public int GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		public long GetInt64(int i)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public string GetName(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}

			return Shared.Constants.ARTIFACT_ID_FIELD_NAME;
		}

		public int GetOrdinal(string name)
		{
			if (name != Shared.Constants.ARTIFACT_ID_FIELD_NAME)
			{
				throw new IndexOutOfRangeException();
			}

			return 0;
		}

		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		public object GetValue(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}

			return _currentDocument.ArtifactID;
		}

		public int GetValues(object[] values)
		{
			// We do not need this at this point
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