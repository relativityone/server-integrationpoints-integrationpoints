using System;
using System.Collections.Generic;
using System.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class DocumentArtifactIdDataReader : IDataReader
	{
		private readonly IRSAPIClient _rsapiClient;
		private readonly int _savedSearchArtifactId;
		private IEnumerator<Result<Document>> _savedSearchResultDocumentsEnumerator;
		private Document _currentDocument;
		private bool _readerOpen;
		private const string ARTIFACT_ID = "ArtifactId";

		public DocumentArtifactIdDataReader(IRSAPIClient proxy, int savedSearchArtifactId)
		{
			_rsapiClient = proxy;
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
			get { return _readerOpen; }
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
					ResultSet<Document> docResults = _rsapiClient.Repositories.Document.Query(query);
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
			else
			{
				if (_savedSearchResultDocumentsEnumerator.Current != null)
				{
					if (_currentDocument == _savedSearchResultDocumentsEnumerator.Current.Artifact)
					{
						_savedSearchResultDocumentsEnumerator.MoveNext();
					}

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
			_savedSearchResultDocumentsEnumerator.Dispose();
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
			return ARTIFACT_ID;
		}

		public System.DateTime GetDateTime(int i)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
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
			return typeof (Int32);
		}

		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
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
			return ARTIFACT_ID;
		}

		public int GetOrdinal(string name)
		{
			return 0;
		}

		public string GetString(int i)
		{
			return Convert.ToString(GetValue(i));
		}

		public object GetValue(int i)
		{
			if (i == 0)
			{
				return _currentDocument.ArtifactID;
			}

			return null;
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
				if (name == ARTIFACT_ID)
				{
					return GetValue(GetOrdinal(name));
				}

				return null;
			}
		}

		public object this[int i]
		{
			get { return GetValue(i); }
		}
	}
}