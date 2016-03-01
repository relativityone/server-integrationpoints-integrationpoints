using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly int[] _documentArtifactIds;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldValueLoader _fieldsLoader;

		public DocumentTransferDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> longTextfieldEntries) :
			base(relativityClientAdaptor, GenerateDataColumnsFromFieldEntries(fieldEntries))
		{
			_documentArtifactIds = documentArtifactIds.ToArray();
			_fieldEntries = fieldEntries.ToList();
			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
			_longTextFieldArtifactIds = new HashSet<int>(longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)));
			_fieldsLoader = new FieldValueLoader(relativityClientAdaptor, longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)).ToArray(),
				_documentArtifactIds);
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(IEnumerable<FieldEntry> fieldEntries)
		{
			return fieldEntries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
		}

		protected override QueryResultSet<Document> ExecuteQueryToGetInitialResult()
		{
			var artifactIdSetCondition = new WholeNumberCondition
			{
				Field = Shared.Constants.ARTIFACT_ID_FIELD_NAME,
				Operator = NumericConditionEnum.In,
				Value = _documentArtifactIds.ToList()
			};

			// only query for non-full text fields at this time
			List<FieldValue> requestedFields = _fieldEntries.Where(field => _longTextFieldArtifactIds.Contains(Convert.ToInt32(field.FieldIdentifier)) == false)
												.Select(x => new FieldValue() { ArtifactID = Convert.ToInt32(x.FieldIdentifier) }).ToList();

			Query<Document> query = new Query<Document>
			{
				Condition = artifactIdSetCondition,
				Fields = requestedFields
			};
			return RelativityClient.ExecuteDocumentQuery(query);
		}

		public override string GetDataTypeName(int i)
		{
			return GetFieldType(i).ToString();
		}

		public override Type GetFieldType(int i)
		{
			string columnName = GetName(i);
			return CurrentDocument[columnName] == null ? typeof(object) : CurrentDocument[columnName].GetType();
		}

		public override object GetValue(int i)
		{
			Object result = null;
			int fieldArtifactId = Convert.ToInt32(GetName(i));

			if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
			}
			else
			{
				result = CurrentDocument[fieldArtifactId].Value;
			}
			return result;
		}


		private int _localCacheId;
		private List<FieldValue> _localCache;
		private string GetLongTextFieldFromPreLoadedCache(int documentArtifactId, int fieldArtifactId)
		{
			if (documentArtifactId != _localCacheId)
			{
				Task<List<FieldValue>> getLongTextFieldsTasks = _fieldsLoader.GetFieldsValue(documentArtifactId);
				try
				{
					getLongTextFieldsTasks.Wait();

					if (getLongTextFieldsTasks.IsCompleted)
					{
						if (getLongTextFieldsTasks.Exception != null)
						{
							throw getLongTextFieldsTasks.Exception;
						}

						_localCacheId = documentArtifactId;
						_localCache = getLongTextFieldsTasks.Result;
					}
				}
				catch (Exception exception)
				{
					throw exception.InnerException ?? exception;
				}
			}
			return _localCache.First(field => field.ArtifactID == fieldArtifactId).ValueAsLongText;
		}

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldFromPreLoadedCache(CurrentDocument.ArtifactID, fieldArtifactId);
		}

	}
}